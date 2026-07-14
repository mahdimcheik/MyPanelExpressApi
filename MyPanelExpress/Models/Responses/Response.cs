using System.ComponentModel.DataAnnotations;

namespace MyPanelExpress.Models;

/// <summary>
/// Enveloppe renvoyée par tous les services. Le contrôleur la repasse telle quelle
/// en JSON avec <c>StatusCode(response.Status, response)</c>.
/// </summary>
public class Response<T>
{
    [Required]
    public string Message { get; set; } = string.Empty;

    [Required]
    public int Status { get; set; }

    public T? Data { get; set; }

    /// <summary>Nombre total de lignes, pour les listes paginées.</summary>
    public long? Count { get; set; }
}
