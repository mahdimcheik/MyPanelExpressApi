namespace MyPanelExpress.Models;

public class MailApp
{
    public string MailTo { get; set; } = string.Empty;
    public string MailFrom { get; set; } = string.Empty;
    public string MailSubject { get; set; } = string.Empty;
    public string MailBody { get; set; } = string.Empty;
    public string? ReplyTo { get; set; }
}

/// <summary>Modèle du template ContactMessageTemplate.cshtml.</summary>
public class ContactMessageModel
{
    public string Nom { get; set; } = string.Empty;
    public string? Societe { get; set; }
    public string? Telephone { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Sujet { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset DateEnvoi { get; set; } = DateTimeOffset.UtcNow;
}
