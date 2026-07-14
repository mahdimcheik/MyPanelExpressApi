using System.Net;
using System.Net.Mail;
using MyPanelExpress.Models;
using MyPanelExpress.Utilities;
using RazorLight;

namespace MyPanelExpress.Services;

/// <summary>
/// Envoi d'emails. Dans ce projet, le seul usage est le formulaire de contact du site public :
/// il n'y a ni inscription, ni confirmation de compte, ni réinitialisation de mot de passe.
/// </summary>
public class MailService
{
    private readonly IRazorLightEngine _razorLightEngine;
    private readonly ILogger<MailService> _logger;

    public MailService(ILogger<MailService> logger)
    {
        _logger = logger;
        _razorLightEngine = new RazorLightEngineBuilder()
            .UseFileSystemProject(Path.Combine(AppContext.BaseDirectory, "Templates"))
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task SendEmailAsync(MailApp mail)
    {
        using var smtpClient = new SmtpClient(EnvironmentVariables.SMTP_HOST)
        {
            Port = EnvironmentVariables.SMTP_PORT,
            EnableSsl = EnvironmentVariables.SMTP_SSL,
        };

        // Mailpit en local n'exige pas d'authentification ; ne présenter des identifiants
        // vides ferait échouer la négociation avec certains serveurs.
        if (!string.IsNullOrWhiteSpace(EnvironmentVariables.SMTP_LOGIN))
        {
            smtpClient.Credentials = new NetworkCredential(
                EnvironmentVariables.SMTP_LOGIN,
                EnvironmentVariables.SMTP_KEY
            );
        }

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(mail.MailFrom),
            Subject = mail.MailSubject,
            Body = mail.MailBody,
            IsBodyHtml = true,
            BodyEncoding = System.Text.Encoding.UTF8,
            SubjectEncoding = System.Text.Encoding.UTF8,
        };

        mailMessage.To.Add(mail.MailTo);

        if (!string.IsNullOrWhiteSpace(mail.ReplyTo))
        {
            mailMessage.ReplyToList.Add(new MailAddress(mail.ReplyTo));
        }

        await smtpClient.SendMailAsync(mailMessage);
    }

    /// <summary>
    /// Transmet un message du formulaire de contact à l'entreprise. Laisse remonter les
    /// exceptions : c'est le service appelant qui décide du code de retour. Avaler l'erreur
    /// ici afficherait une confirmation au visiteur alors que le message est perdu.
    /// </summary>
    public async Task SendContactMessageAsync(ContactMessageModel model)
    {
        var body = await _razorLightEngine.CompileRenderAsync("ContactMessageTemplate.cshtml", model);

        await SendEmailAsync(
            new MailApp
            {
                MailFrom = EnvironmentVariables.DO_NO_REPLY_MAIL,
                MailTo = EnvironmentVariables.CONTACT_RECIPIENT_MAIL,
                MailSubject = $"Nouveau message de contact : {model.Sujet}",
                MailBody = body,
                // Pour répondre au visiteur d'un clic depuis la boîte mail.
                ReplyTo = model.Email,
            }
        );

        _logger.LogInformation("Message de contact envoyé de {Email}", model.Email);
    }
}
