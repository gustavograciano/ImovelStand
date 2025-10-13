// Script para gerar hashes BCrypt
// Executar com: dotnet script generate-hashes.csx

#r "nuget: BCrypt.Net-Next, 4.0.3"

using BCrypt.Net;

Console.WriteLine("Hash para Admin@123:");
Console.WriteLine(BCrypt.HashPassword("Admin@123"));

Console.WriteLine("\nHash para Corretor@123:");
Console.WriteLine(BCrypt.HashPassword("Corretor@123"));
