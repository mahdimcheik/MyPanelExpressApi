using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPanelExpress.Models.Interfaces;

public interface IArchivable
{
    public DateTimeOffset? ArchivedAt { get; set; }
}

public interface IUpdateable
{
    public DateTimeOffset? UpdatedAt { get; set; }
}

public interface ICreatable
{
    public DateTimeOffset CreatedAt { get; set; }
}

public abstract class BaseModel : IUpdateable, ICreatable, IArchivable
{
    [Key]
    public Guid Id { get; set; }

    // UtcNow, pas Now : Npgsql refuse d'écrire un DateTimeOffset dont l'Offset != 0 dans une
    // colonne "timestamp with time zone" (InvalidCastException). DateTimeOffset.Now porte
    // l'offset local (+01:00 en France) et fait donc échouer chaque insertion. Ne pas "corriger".
    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset? ArchivedAt { get; set; }
}
