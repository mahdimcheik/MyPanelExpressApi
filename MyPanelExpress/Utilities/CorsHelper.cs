namespace MyPanelExpress.Utilities;

public static class CorsHelper
{
    public static bool IsOriginAllowed(string origin)
    {
        List<string> allowedOrigins =
        [
            // Angular dev server
            "http://localhost:4200",
            "https://localhost:4200",
            // cette API (pour que Swagger UI puisse s'appeler lui-même)
            "http://localhost:5196",
            "https://localhost:7026",
            EnvironmentVariables.API_FRONT_URL,
            EnvironmentVariables.API_BACK_URL,
        ];

        return allowedOrigins.Contains(origin);
    }
}
