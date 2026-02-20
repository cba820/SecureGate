namespace Gateway.Infrastructure.Database.Options;

public sealed class DatabaseOptions {
    public string Provider { get; init; } = "Sqlite"; // Postgres | Sqlite

    public PostgresOptions Postgres { get; init; } = new();
    public SqliteOptions Sqlite { get; init; } = new();

    public sealed class PostgresOptions {
        public string ConnectionString { get; init; } = string.Empty;
    }

    public sealed class SqliteOptions {
        public string ConnectionString { get; init; } = "Data Source=./data/gateway.db";
    }
}
