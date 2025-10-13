using BCrypt.Net;

Console.WriteLine("=== Gerando Hashes BCrypt ===\n");

Console.WriteLine("Hash para Admin@123:");
var adminHash = BCrypt.Net.BCrypt.HashPassword("Admin@123", 11);
Console.WriteLine(adminHash);

Console.WriteLine("\nHash para Corretor@123:");
var corretorHash = BCrypt.Net.BCrypt.HashPassword("Corretor@123", 11);
Console.WriteLine(corretorHash);

Console.WriteLine("\n=== Hashes Gerados com Sucesso! ===");
