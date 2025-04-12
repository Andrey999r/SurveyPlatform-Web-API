using Backend.Core.DTO.Survey;

namespace Backend.Core.Interfaces;

public interface ISurveyService
{
    // Создание опроса
    int CreateSurvey(CreateSurveyDto dto, int userId);

    // Получить список опросов, созданных пользователем
    List<SurveyListItemDto> GetSurveysByUser(int userId);

    // Получить список опросов, в которых пользователь (с userId) является участником
    List<SurveyWithAnswersDto> GetCompletedSurveys(int userId);

    // Получить детальную информацию по конкретному опросу
    SurveyDetailsDto? GetSurveyDetails(int surveyId, int userId);

    // Удалить опрос (и связанные данные), если он принадлежит userId
    void DeleteSurvey(int surveyId, int userId);

    // Сгенерировать/вернуть ссылку на прохождение опроса (может быть просто строка)
    string GenerateSurveyLink(int surveyId);

    // Отправить приглашение на e-mail
    void SendSurveyInvitation(int surveyId, string recipientEmail);

    // Получить данные для прохождения опроса (вопросы и т. п.)
    SurveyTakeDto? GetSurveyForTake(int surveyId);

    // Сохранить ответы участника (TakeSurvey POST)
    void SaveSurveyAnswers(int surveyId, string participantName, string participantEmail, string[] answers, int? userId);

    // Обновить email участника
    void UpdateParticipantEmail(UpdateEmailDto model);

    // Получить информацию об участнике (его ответы и пр.)
    ParticipantInfoDto? GetParticipantInfo(int participantId);

    // Удалить участника опроса
    void DeleteParticipant(int participantId);

    // Получить подробную информацию об опросе и ответах конкретного участника
    ParticipantInfoSurveyDto? GetParticipantSurveyInfo(int participantId);

    // Метод для возвращения благодарности (может быть не нужен как отдельный сервис)
    // или вы можете просто вернуть статический текст/данные
}