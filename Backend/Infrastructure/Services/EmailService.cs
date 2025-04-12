using System;
using System.Net;
using System.Net.Mail;
using Backend.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Backend.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void SendSurveyInvitation(string recipientEmail, string surveyLink)
    {
        // Пример упрощённой отправки
        var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "";
        var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "";
        var senderPassword = _configuration["EmailSettings:SenderPassword"] ?? "";

        using var client = new SmtpClient(smtpServer, smtpPort)
        {
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true,
            Timeout = 20000
        };

        var mail = new MailMessage
        {
            From = new MailAddress(senderEmail),
            Subject = "Приглашение пройти опрос",
            Body = $"Пройдите опрос по ссылке: {surveyLink}",
            IsBodyHtml = true
        };
        mail.To.Add(recipientEmail);

        client.Send(mail);
    }
}