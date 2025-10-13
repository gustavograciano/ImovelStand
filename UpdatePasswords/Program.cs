using Microsoft.Data.SqlClient;

var connectionString = "Server=GUSTAVO\\MSSQLSERVER01;Database=ImovelStandDb;User Id=sa;Password=REPLACE_VIA_ENV_VAR;TrustServerCertificate=True;";

Console.WriteLine("=== Atualizando senhas dos usuários ===\n");

try
{
    using var connection = new SqlConnection(connectionString);
    connection.Open();

    // Atualizar senha do Admin
    var adminHash = "$2a$11$r4aHE2PnR4xi9noJxkzqe.2SIC5DqPZvinTi8EmFOHsMRIWcrPkqi";
    using (var cmd = new SqlCommand("UPDATE Usuarios SET SenhaHash = @hash WHERE Id = 1", connection))
    {
        cmd.Parameters.AddWithValue("@hash", adminHash);
        var rows = cmd.ExecuteNonQuery();
        Console.WriteLine($"Admin: {rows} registro(s) atualizado(s)");
    }

    // Atualizar senha do Corretor
    var corretorHash = "$2a$11$D8sg3FrM1EI689Z905iG2ubYw/m6LSlI3au9TWZFWd9dCFhw9rxQS";
    using (var cmd = new SqlCommand("UPDATE Usuarios SET SenhaHash = @hash WHERE Id = 2", connection))
    {
        cmd.Parameters.AddWithValue("@hash", corretorHash);
        var rows = cmd.ExecuteNonQuery();
        Console.WriteLine($"Corretor: {rows} registro(s) atualizado(s)");
    }

    Console.WriteLine("\n=== Senhas atualizadas com sucesso! ===");
    Console.WriteLine("\nCredenciais válidas:");
    Console.WriteLine("- Admin: admin@imovelstand.com / Admin@123");
    Console.WriteLine("- Corretor: corretor@imovelstand.com / Corretor@123");
}
catch (Exception ex)
{
    Console.WriteLine($"Erro: {ex.Message}");
}
