using System.Linq;
using Backend.Core.DTO.Survey;
using Backend.Core.Entities;
using Backend.Core.Exceptions;
using Backend.Core.Interfaces;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Services;

public class SurveyService : ISurveyService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public SurveyService(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // Создание опроса
    public int CreateSurvey(CreateSurveyDto dto, int userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new DomainException("Название опроса не может быть пустым.");

        var user = _context.Users.Find(userId);
        if (user == null)
            throw new NotFoundException("Пользователь не найден.");

        var survey = new Survey
        {
            Name = dto.Name,
            Description = dto.Description,
            ApplicationUserId = userId
        };

        if (dto.Questions != null)
        {
            foreach (var qText in dto.Questions.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                survey.Questions.Add(new Question { Text = qText });
            }
        }

        _context.Surveys.Add(survey);
        _context.SaveChanges();
        return survey.Id;
    }

    // Получить список опросов, созданных пользователем
    public List<SurveyListItemDto> GetSurveysByUser(int userId)
    {
        return _context.Surveys
            .Where(s => s.ApplicationUserId == userId)
            .Select(s => new SurveyListItemDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description
            })
            .ToList();
    }

    // Получить список опросов, в которых userId является участником (Completed)
    public List<SurveyWithAnswersDto> GetCompletedSurveys(int userId)
    {
        // Узнаём email пользователя
        var userEmail = _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Email)
            .FirstOrDefault();

        if (string.IsNullOrEmpty(userEmail))
            return new List<SurveyWithAnswersDto>(); // или бросать ошибку

        // Опросы, где есть участник с этим email
        var completed = _context.Surveys
            .Include(s => s.Owner)
            .Include(s => s.Participants)
                .ThenInclude(p => p.Answers)
                .ThenInclude(a => a.Question)
            .Where(s => s.Participants.Any(p => p.Email == userEmail))
            .ToList();

        // Преобразуем в DTO
        var result = completed.Select(s => new SurveyWithAnswersDto
        {
            SurveyId = s.Id,
            Name = s.Name,
            OwnerName = s.Owner != null ? s.Owner.Username : "Unknown",
            // Находим конкретного участника
            CompletedAt = s.Participants
                .FirstOrDefault(p => p.Email == userEmail)?.CompletedAt ?? DateTime.UtcNow,
            Answers = s.Participants
                .Where(p => p.Email == userEmail)
                .SelectMany(p => p.Answers)
                .Select(a => new AnswerDto
                {
                    QuestionText = a.Question != null ? a.Question.Text : "",
                    ResponseText = a.ResponseText
                })
                .ToList()
        }).ToList();

        return result;
    }

    // Получить детальную информацию по конкретному опросу
    public SurveyDetailsDto? GetSurveyDetails(int surveyId, int userId)
    {
        var survey = _context.Surveys
            .Include(s => s.Questions)
            .Include(s => s.Participants)
                .ThenInclude(p => p.Answers)
                .ThenInclude(a => a.Question)
            .FirstOrDefault(s => s.Id == surveyId && s.ApplicationUserId == userId);

        if (survey == null) return null;

        return new SurveyDetailsDto
        {
            Name = survey.Name,
            Description = survey.Description,
            Questions = survey.Questions
                .Select(q => new QuestionDto { Id = q.Id, Text = q.Text })
                .ToList(),
            Participants = survey.Participants
                .Select(p => new ParticipantDto
                {
                    ParticipantName = p.ParticipantName,
                    Email = p.Email,
                    CompletedAt = p.CompletedAt,
                    Answers = p.Answers.Select(a => new AnswerDto
                    {
                        QuestionText = a.Question?.Text ?? "",
                        ResponseText = a.ResponseText
                    }).ToList()
                })
                .ToList()
        };
    }

    // Удалить опрос
    public void DeleteSurvey(int surveyId, int userId)
    {
        var survey = _context.Surveys
            .Include(s => s.Participants)
                .ThenInclude(p => p.Answers)
            .FirstOrDefault(s => s.Id == surveyId && s.ApplicationUserId == userId);

        if (survey == null)
            throw new NotFoundException("Опрос не найден или не принадлежит пользователю.");

        foreach (var participant in survey.Participants)
        {
            _context.Answers.RemoveRange(participant.Answers);
        }
        _context.Participants.RemoveRange(survey.Participants);
        _context.Surveys.Remove(survey);
        _context.SaveChanges();
    }

    // Сгенерировать ссылку (просто пример - формировать реальную ссылку нужно в контроллере)
    public string GenerateSurveyLink(int surveyId)
    {
        // В реальном приложении: builder.Url.Action(...), но тут просто заглушка
        return $"https://yourdomain.com/api/surveys/{surveyId}/take";
    }

    // Отправить приглашение на e-mail
    public void SendSurveyInvitation(int surveyId, string recipientEmail)
    {
        var link = GenerateSurveyLink(surveyId);
        // Просто вызываем EmailService
        _emailService.SendSurveyInvitation(recipientEmail, link);
    }

    // Получить данные для прохождения опроса
    public SurveyTakeDto? GetSurveyForTake(int surveyId)
    {
        var survey = _context.Surveys
            .Include(s => s.Questions)
            .FirstOrDefault(s => s.Id == surveyId);

        if (survey == null) return null;

        return new SurveyTakeDto
        {
            SurveyId = survey.Id,
            Name = survey.Name,
            Description = survey.Description,
            Questions = survey.Questions
                .Select(q => new QuestionDto { Id = q.Id, Text = q.Text })
                .ToList()
        };
    }

    // Сохранить ответы участника
    public void SaveSurveyAnswers(int surveyId, string participantName, string participantEmail, string[] answers, int? userId)
    {
        var survey = _context.Surveys
            .FirstOrDefault(s => s.Id == surveyId);
        if (survey == null)
            throw new NotFoundException("Опрос не найден.");

        if (answers == null || answers.Length == 0)
            throw new DomainException("Ответы не найдены.");

        // Если пользователь авторизован, получаем Email из БД
        string? userEmail = null;
        if (userId.HasValue)
        {
            userEmail = _context.Users
                .Where(u => u.Id == userId.Value)
                .Select(u => u.Email)
                .FirstOrDefault();
        }
        // Смотрим, был ли уже участник
        var existingParticipant = _context.Participants
            .FirstOrDefault(p =>
                p.SurveyId == surveyId &&
                (p.Email == userEmail || p.Email == participantEmail));

        if (existingParticipant == null)
        {
            existingParticipant = new Participant
            {
                ParticipantName = participantName ?? "Аноним",
                Email = userEmail ?? participantEmail,
                SurveyId = surveyId,
                CompletedAt = DateTime.UtcNow.AddHours(3)
            };
            _context.Participants.Add(existingParticipant);
            _context.SaveChanges();
        }

        // Находим вопросы данного опроса
        var surveyQuestions = _context.Questions
            .Where(q => q.SurveyId == surveyId)
            .ToList();

        // Сохраняем ответы
        for (int i = 0; i < surveyQuestions.Count && i < answers.Length; i++)
        {
            var answer = new Answer
            {
                QuestionId = surveyQuestions[i].Id,
                ParticipantId = existingParticipant.Id,
                ResponseText = answers[i]
            };
            _context.Answers.Add(answer);
        }
        _context.SaveChanges();
    }

    // Обновить email участника
    public void UpdateParticipantEmail(UpdateEmailDto model)
    {
        var participant = _context.Participants.Find(model.ParticipantId);
        if (participant == null)
            throw new NotFoundException("Участник не найден.");

        participant.Email = model.NewEmail;
        _context.SaveChanges();
    }

    // Получить информацию об участнике
    public ParticipantInfoDto? GetParticipantInfo(int participantId)
    {
        var participant = _context.Participants
            .Include(p => p.Answers)
            .ThenInclude(a => a.Question)
            .FirstOrDefault(p => p.Id == participantId);
        if (participant == null) return null;

        var dto = new ParticipantInfoDto
        {
            ParticipantId = participant.Id,
            ParticipantName = participant.ParticipantName,
            Email = participant.Email,
            Answers = participant.Answers.Select(a => new AnswerDto
            {
                QuestionText = a.Question?.Text ?? "",
                ResponseText = a.ResponseText
            }).ToList()
        };
        return dto;
    }

    // Удалить участника
    public void DeleteParticipant(int participantId)
    {
        var participant = _context.Participants
            .Include(p => p.Answers)
            .FirstOrDefault(p => p.Id == participantId);
        if (participant == null)
            throw new NotFoundException("Участник не найден.");

        _context.Answers.RemoveRange(participant.Answers);
        _context.Participants.Remove(participant);
        _context.SaveChanges();
    }

    // Подробная информация об опросе и ответах конкретного участника
    public ParticipantInfoSurveyDto? GetParticipantSurveyInfo(int participantId)
    {
        var participant = _context.Participants
            .Include(p => p.Survey)
            .Include(p => p.Answers)
                .ThenInclude(a => a.Question)
            .FirstOrDefault(p => p.Id == participantId);
        if (participant == null) return null;

        var dto = new ParticipantInfoSurveyDto
        {
            ParticipantId = participant.Id,
            ParticipantName = participant.ParticipantName,
            Email = participant.Email,
            SurveyName = participant.Survey?.Name ?? "Неизвестный опрос",
            Answers = participant.Answers.Select(a => new AnswerDto
            {
                QuestionText = a.Question?.Text ?? "",
                ResponseText = a.ResponseText
            }).ToList()
        };
        return dto;
    }
}