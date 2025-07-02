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
}