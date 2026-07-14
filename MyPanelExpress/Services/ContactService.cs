using MyPanelExpress.Models;
using MyPanelExpress.Services.Interfaces;

namespace MyPanelExpress.Services;

public class ContactService(MailService mailService, ILogger<ContactService> logger)
    : IContactService
{
    public async Task<Response<object>> SendMessageAsync(CreateContactMessage dto)
    {
        try
        {
            // Pot de miel rempli : c'est un robot. On renvoie un succès sans rien envoyer,
            // pour ne pas lui indiquer qu'il a été repéré.
            if (!string.IsNullOrWhiteSpace(dto.Website))
            {
                logger.LogWarning("Message de contact rejeté (pot de miel) de {Email}", dto.Email);

                return new Response<object>
                {
                    Status = 200,
                    Message = "Votre message a bien été envoyé",
                    Data = null,
                };
            }

            await mailService.SendContactMessageAsync(
                new ContactMessageModel
                {
                    Nom = dto.Nom,
                    Societe = dto.Societe,
                    Telephone = dto.Telephone,
                    Email = dto.Email,
                    Sujet = dto.Sujet,
                    Message = dto.Message,
                    DateEnvoi = DateTimeOffset.UtcNow,
                }
            );

            return new Response<object>
            {
                Status = 200,
                Message = "Votre message a bien été envoyé",
                Data = null,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de l'envoi du message de contact");

            return new Response<object>
            {
                Status = 500,
                Message = "Erreur lors de l'envoi du message. Veuillez réessayer plus tard.",
                Data = null,
            };
        }
    }
}
