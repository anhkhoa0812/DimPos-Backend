using System.Security.Cryptography;
using System.Text;

namespace DimPos.Payment.Application.Common.Utils;

public static class CryptographyUtil
{
    public static string Encode(string plainText, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);

        using (Aes aes = Aes.Create())
        {
            aes.Mode = CipherMode.ECB;
            aes.Key = keyBytes;
            aes.Padding = PaddingMode.PKCS7;
            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            {
                byte[] cipherBytes = encryptor.TransformFinalBlock(
                    plaintextBytes, 0, plaintextBytes.Length);
        
                return Convert.ToBase64String(cipherBytes);
            }
        }
    }

    public static string Decode(string base64Text, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] cipherBytes = Convert.FromBase64String(base64Text);

        using (Aes aes = Aes.Create())
        {
            aes.Mode = CipherMode.ECB;
            aes.Key = keyBytes;
            aes.Padding = PaddingMode.PKCS7;
            using (ICryptoTransform decryptor = aes.CreateDecryptor())
            {
                byte[] plaintextBytes = decryptor.TransformFinalBlock(
                    cipherBytes, 0, cipherBytes.Length);
        
                return Encoding.UTF8.GetString(plaintextBytes);   
            }
        }
    }
    private static byte[] GetAesKey(string keyString)
    {
        // 1. Loại bỏ hyphen (GUID.ToString("D") => 36 ký tự, có '-')
        var normalized = keyString.Replace("-", "");

        // 2. Lấy UTF8 bytes
        var bytes = Encoding.UTF8.GetBytes(normalized);

        // 3. Nếu đúng 16/24/32 => dùng luôn
        if (bytes.Length == 16 || bytes.Length == 24 || bytes.Length == 32)
        {
            return bytes;
        }

        // 4. Ngược lại hash SHA256 để luôn có 32 bytes
        using var sha = SHA256.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(normalized));
    }
    public static string DecodeCredentialsConfig(string data, string key)
    {
        var keyBytes    = GetAesKey(key);
        var cipherBytes = Convert.FromBase64String(data);

        using var aes = Aes.Create();
        aes.Mode    = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key     = keyBytes;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}