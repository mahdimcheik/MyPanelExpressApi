using DotNetEnv;

namespace MyPanelExpress.Utilities;

public static class DotEnv
{
    /// <summary>
    /// Charge le premier fichier .env trouvé en remontant depuis le répertoire de l'assembly,
    /// et pousse ses valeurs dans les variables d'environnement du process.
    /// <para>
    /// On remonte l'arborescence au lieu de lire "./.env" parce que le répertoire courant change
    /// selon la façon de lancer : `dotnet run --project` (racine de la solution), F5 depuis Visual
    /// Studio (dossier du projet), `dotnet ef` (n'importe où). Sans fichier, on ne fait rien :
    /// c'est le cas du conteneur, où les variables sont déjà injectées.
    /// </para>
    /// </summary>
    public static void Load()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, ".env");
            if (File.Exists(candidate))
            {
                Env.Load(candidate);
                return;
            }

            directory = directory.Parent;
        }
    }
}
