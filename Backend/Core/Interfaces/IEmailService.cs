namespace Backend.Core.Interfaces;

public interface IEmailService
{
    void SendSurveyInvitation(string recipientEmail, string surveyLink);
}