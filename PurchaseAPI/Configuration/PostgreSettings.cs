namespace PurchaseAPI.Configuration
{
    public class PostgreSettings
    {
        public string? Host { get; set; }
        public string? Port { get; set; }
        public string? Database { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }

        public static PostgreSettings FromEnvironmentOrConfig(IConfiguration configuration)
        {
            var baseConnectionString = configuration.GetConnectionString("DefaultConnection");
            var settings = ParseConnectionString(baseConnectionString);

            settings.Host = Environment.GetEnvironmentVariable("DB_HOST") ?? settings.Host;
            settings.Port = Environment.GetEnvironmentVariable("DB_PORT") ?? settings.Port;
            settings.Username = Environment.GetEnvironmentVariable("DB_USER") ?? settings.Username;
            settings.Password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? settings.Password;
            settings.Database = Environment.GetEnvironmentVariable("DB_NAME") ?? settings.Database;

            return settings;
        }

        public string BuildConnectionString() =>
            $"Host={Host ?? "localhost"};Port={Port ?? "5432"};Database={Database ?? "PurchaseDb"};Username={Username ?? "postgres"};Password={Password ?? ""}";

        private static PostgreSettings ParseConnectionString(string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) return new PostgreSettings();

            var settings = new PostgreSettings();
            var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var kv = part.Split('=', 2);
                if (kv.Length != 2) continue;

                switch (kv[0].Trim().ToLower())
                {
                    case "host": settings.Host = kv[1].Trim(); break;
                    case "port": settings.Port = kv[1].Trim(); break;
                    case "database": settings.Database = kv[1].Trim(); break;
                    case "username": settings.Username = kv[1].Trim(); break;
                    case "password": settings.Password = kv[1].Trim(); break;
                }
            }

            return settings;
        }
    }
}
