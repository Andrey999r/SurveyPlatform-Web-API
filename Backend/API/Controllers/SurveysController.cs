using Backend.Core.DTO.Survey;
using Backend.Core.Exceptions;
using Backend.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SurveysController : ControllerBase
{
    private readonly ISurveyService _surveyService;

    public SurveysController(ISurveyService surveyService)
    {
        _surveyService = surveyService;
    }

    // 1) Создать опрос
    [HttpPost("create")]
    public IActionResult CreateSurvey([FromBody] CreateSurveyDto dto)
    {
        int userId = GetUserIdFromToken();
        try
        {
            var surveyId = _surveyService.CreateSurvey(dto, userId);
            return Ok(new { SurveyId = surveyId });
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // 2) Получить список созданных опросов
    [HttpGet("created")]
    public IActionResult GetCreatedSurveys()
    {
        int userId = GetUserIdFromToken();
        var surveys = _surveyService.GetSurveysByUser(userId);
        return Ok(surveys);
    }

    // 3) Посмотреть список опросов, в которых пользователь участвовал
    [HttpGet("completed")]
    public IActionResult GetCompletedSurveys()
    {
        int userId = GetUserIdFromToken();
        var completed = _surveyService.GetCompletedSurveys(userId);
        return Ok(completed);
    }

    // 4) Детальная информация об опросе
    [HttpGet("{surveyId}/details")]
    public IActionResult Details(int surveyId)
    {
        int userId = GetUserIdFromToken();
        var details = _surveyService.GetSurveyDetails(surveyId, userId);
        if (details == null)
            return NotFound("Опрос не найден или не принадлежит пользователю.");
        return Ok(details);
    }

    // 5) Удалить опрос
    [HttpDelete("{surveyId}")]
    public IActionResult DeleteSurvey(int surveyId)
    {
        int userId = GetUserIdFromToken();
        try
        {
            _surveyService.DeleteSurvey(surveyId, userId);
            return Ok("Опрос удалён.");
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // 6) Сгенерировать ссылку (Share)
    [HttpGet("{surveyId}/share")]
    public IActionResult ShareSurvey(int surveyId)
    {
        var link = _surveyService.GenerateSurveyLink(surveyId);
        return Ok(new { Link = link });
    }

    // 7) Отправить приглашение
    [HttpPost("{surveyId}/invite")]
    public IActionResult SendSurveyInvitation(int surveyId, [FromBody] InviteDto invite)
    {
        try
        {
            _surveyService.SendSurveyInvitation(surveyId, invite.RecipientEmail);
            return Ok("Приглашение отправлено.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // 8) GET TakeSurvey
    [HttpGet("{surveyId}/take")]
    public IActionResult TakeSurveyGet(int surveyId)
    {
        var survey = _surveyService.GetSurveyForTake(surveyId);
        if (survey == null)
            return NotFound("Опрос не найден.");
        return Ok(survey);
    }

    // 9) POST TakeSurvey
    [HttpPost("{surveyId}/take")]
    public IActionResult TakeSurveyPost(int surveyId, [FromBody] TakeSurveyAnswersDto answersDto)
    {
        int? userId = GetOptionalUserIdFromToken();
        try
        {
            _surveyService.SaveSurveyAnswers(surveyId,
                                             answersDto.ParticipantName,
                                             answersDto.ParticipantEmail,
                                             answersDto.Answers,
                                             userId);
            return Ok("Спасибо за прохождение опроса!");
        }
        catch (NotFoundException ex) // сперва дочернее исключение
        {
            return NotFound(ex.Message);
        }
        catch (DomainException ex)   // затем более общий
        {
            return BadRequest(ex.Message);
        }
    }

    // 10) Обновить email участника
    [HttpPost("participants/update-email")]
    public IActionResult UpdateEmail([FromBody] UpdateEmailDto model)
    {
        try
        {
            _surveyService.UpdateParticipantEmail(model);
            return Ok("Email обновлён.");
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // 11) Информация об участнике
    [HttpGet("participants/{participantId}/info")]
    public IActionResult GetParticipantInfo(int participantId)
    {
        var info = _surveyService.GetParticipantInfo(participantId);
        if (info == null)
            return NotFound("Участник не найден.");
        return Ok(info);
    }

    // 12) Удалить участника
    [HttpDelete("participants/{participantId}")]
    public IActionResult DeleteParticipant(int participantId)
    {
        try
        {
            _surveyService.DeleteParticipant(participantId);
            return Ok("Участник успешно удалён.");
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // 13) Подробная информация об участнике и опросе
    [HttpGet("participants/{participantId}/infosurvey")]
    public IActionResult InfoSurvey(int participantId)
    {
        var data = _surveyService.GetParticipantSurveyInfo(participantId);
        if (data == null)
            return NotFound("Участник не найден.");
        return Ok(data);
    }

    // 14) ThankYou
    [HttpGet("thankyou")]
    public IActionResult ThankYou([FromQuery] string participantName)
    {
        return Ok($"Спасибо, {participantName}, что прошли опрос!");
    }

    // helpers
    private int GetUserIdFromToken()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }

    private int? GetOptionalUserIdFromToken()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
        if (userIdClaim == null) return null;
        return int.Parse(userIdClaim.Value);
    }
}