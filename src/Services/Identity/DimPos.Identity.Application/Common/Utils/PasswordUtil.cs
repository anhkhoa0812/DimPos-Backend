using System.Security.Cryptography;

namespace DimPos.Identity.Application.Common.Utils;

public static class PasswordUtil
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 10000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;
    
    public static (string passwordHash, string passwordSalt) HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);
        
        return (Convert.ToHexString(hash), Convert.ToHexString(salt));
    }
    
    public static bool Verify(string password, string passwordHash, string passwordSalt)
    {
        var salt = Convert.FromHexString(passwordSalt);
        var hash = Convert.FromHexString(passwordHash);
        
        byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return hash.SequenceEqual(inputHash);
    }
}