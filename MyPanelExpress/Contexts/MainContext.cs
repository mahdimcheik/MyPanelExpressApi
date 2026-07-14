using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyPanelExpress.Models;

namespace MyPanelExpress.Contexts;

/// <summary>
/// IdentityUserContext (et non IdentityDbContext) : le site n'a pas de rôles, donc on évite
/// de créer les tables AspNetRoles / AspNetUserRoles / AspNetRoleClaims, qui ne serviraient
/// jamais. Il n'y a qu'un seul utilisateur, et il est administrateur par définition.
/// </summary>
public class MainContext(DbContextOptions<MainContext> options)
    : IdentityUserContext<UserApp, Guid>(options)
{
    public DbSet<Realisation> Realisations { get; set; }
    public DbSet<RealisationImage> RealisationImages { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<ServiceImage> ServiceImages { get; set; }
    public DbSet<Horaire> Horaires { get; set; }

    /// <summary>
    /// Date figée pour les lignes semées : EF exige des valeurs déterministes dans HasData.
    /// Un DateTimeOffset.UtcNow ferait apparaître une modification de modèle à chaque build.
    /// </summary>
    private static readonly DateTimeOffset SeedDate = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Suppression réelle des réalisations et des services : les lignes d'images partent avec
        // le parent. Les objets MinIO correspondants sont retirés par la couche service.
        builder.Entity<RealisationImage>()
            .HasOne(i => i.Realisation)
            .WithMany(r => r.Images)
            .HasForeignKey(i => i.RealisationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ServiceImage>()
            .HasOne(i => i.Service)
            .WithMany(s => s.Images)
            .HasForeignKey(i => i.ServiceId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Les endpoints publics filtrent sur Visible, et jamais sur ArchivedAt : la suppression
        // est réelle. Pas de HasQueryFilter ici, contrairement au projet de référence.
        builder.Entity<Realisation>()
            .HasIndex(r => new { r.Visible, r.DateRealisation });

        builder.Entity<Service>()
            .HasIndex(s => new { s.Visible, s.Ordre });

        // Une seule ligne d'horaires par jour.
        builder.Entity<Horaire>()
            .HasIndex(h => h.JourSemaine)
            .IsUnique();

        SeedHoraires(builder);
    }

    /// <summary>
    /// Les sept jours de la semaine, créés une fois pour toutes par la migration : l'administrateur
    /// modifie ces lignes, il n'en crée ni n'en supprime. Par défaut, ouvert du lundi au vendredi,
    /// le samedi matin, et fermé le dimanche.
    /// </summary>
    private static void SeedHoraires(ModelBuilder builder)
    {
        var ouverture = new TimeOnly(8, 0);
        var fermeture = new TimeOnly(18, 0);
        var fermetureSamedi = new TimeOnly(13, 0);

        static Horaire Jour(string id, DayOfWeek jour, bool ferme, TimeOnly? debut, TimeOnly? fin) => new()
        {
            Id = Guid.Parse(id),
            JourSemaine = jour,
            HeureOuverture = debut,
            HeureFermeture = fin,
            Ferme = ferme,
            Visible = true,
            CreatedAt = SeedDate
        };

        builder.Entity<Horaire>().HasData(
            Jour("a1000000-0000-0000-0000-000000000000", DayOfWeek.Sunday, true, null, null),
            Jour("a1000000-0000-0000-0000-000000000001", DayOfWeek.Monday, false, ouverture, fermeture),
            Jour("a1000000-0000-0000-0000-000000000002", DayOfWeek.Tuesday, false, ouverture, fermeture),
            Jour("a1000000-0000-0000-0000-000000000003", DayOfWeek.Wednesday, false, ouverture, fermeture),
            Jour("a1000000-0000-0000-0000-000000000004", DayOfWeek.Thursday, false, ouverture, fermeture),
            Jour("a1000000-0000-0000-0000-000000000005", DayOfWeek.Friday, false, ouverture, fermeture),
            Jour("a1000000-0000-0000-0000-000000000006", DayOfWeek.Saturday, false, ouverture, fermetureSamedi)
        );
    }
}
