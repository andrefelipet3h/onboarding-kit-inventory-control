using System.Security.Cryptography;

namespace ControleEstoque.Models;

/// <summary>
/// Gera e confere o hash das senhas (PBKDF2). No banco fica só o hash.
/// </summary>
public static class Senha
{
    private const int Iteracoes = 100000;
    private const int TamanhoSal = 16;
    private const int TamanhoHash = 32;

    public static string GerarHash(string senha)
    {
        byte[] sal = RandomNumberGenerator.GetBytes(TamanhoSal);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(senha, sal, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);
        return $"{Iteracoes}.{Convert.ToBase64String(sal)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>Confere a senha digitada com o hash gravado.</summary>
    public static bool Confere(string senha, string senhaHash)
    {
        if (string.IsNullOrEmpty(senha) || string.IsNullOrEmpty(senhaHash))
            return false;

        string[] partes = senhaHash.Split('.');

        if (partes.Length != 3 || !int.TryParse(partes[0], out int iteracoes))
            return false;

        try
        {
            byte[] sal = Convert.FromBase64String(partes[1]);
            byte[] esperado = Convert.FromBase64String(partes[2]);
            byte[] calculado = Rfc2898DeriveBytes.Pbkdf2(senha, sal, iteracoes, HashAlgorithmName.SHA256, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
