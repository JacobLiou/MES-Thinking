// See https://aka.ms/new-console-template for more information
using Npgsql;

const string connectionString =
    """
    Host=ep-dry-breeze-aout5lc8.c-2.ap-southeast-1.aws.neon.tech;
    Port=5432;
    Database=neondb;
    Username=neondb_owner;
    Password=npg_Du6kACGEoX4e;
    SSL Mode=Require;
    Trust Server Certificate=true;
    Timeout=15;
    Command Timeout=15;
    """;

try
{
    Console.WriteLine("Connecting...");

    await using var conn = new NpgsqlConnection(connectionString);

    await conn.OpenAsync();

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Connected!");
    Console.ResetColor();

    Console.WriteLine($"Server Version: {conn.PostgreSqlVersion}");

    await using var cmd = new NpgsqlCommand(
        """
        SELECT
            now(),
            current_database(),
            current_user;
        """,
        conn);

    await using var reader = await cmd.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        Console.WriteLine($"Time     : {reader.GetDateTime(0)}");
        Console.WriteLine($"Database : {reader.GetString(1)}");
        Console.WriteLine($"User     : {reader.GetString(2)}");
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("FAILED!");
    Console.WriteLine(ex.ToString());
    Console.ResetColor();

    Console.WriteLine("=== Type ===");
    Console.WriteLine(ex.GetType().FullName);

    Console.WriteLine("=== Message ===");
    Console.WriteLine(ex.Message);

    Console.WriteLine("=== Full ===");
    Console.WriteLine(ex.ToString());

    Console.WriteLine("=== Inner ===");
    Console.WriteLine(ex.InnerException?.ToString());
}

