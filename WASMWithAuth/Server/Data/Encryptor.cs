using System.Security.Cryptography;
using System.Text;
using System.Web;
using WASMWithAuth.Shared.Entities.Models;

namespace WASMWithAuth.Server.Data;

public static class Encryptor
{
    public static string Encrypt(TokenKeyModel request)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(request.key);
        aes.GenerateIV();
        var iv = aes.IV;

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var memoryStream = new MemoryStream();
        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        {
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(request.token);
            }
        }
        var cipherText = memoryStream.ToArray();
        var resultBytes = new byte[iv.Length + cipherText.Length];
        Buffer.BlockCopy(iv, 0, resultBytes, 0, iv.Length);
        Buffer.BlockCopy(cipherText, 0, resultBytes, iv.Length, cipherText.Length);
        var base64text = Convert.ToBase64String(resultBytes);
        return HttpUtility.UrlEncode(base64text);
    }

    public static string Decrypt(TokenKeyModel request)
    {
        try
        {
            //var decodedtext = HttpUtility.UrlDecode(request.token);
            var iv = new byte[16];
            var buffer = Convert.FromBase64String(request.token);

            Buffer.BlockCopy(buffer, 0, iv, 0, iv.Length);

            var cipherBytes = new byte[buffer.Length - iv.Length];
            Buffer.BlockCopy(buffer, iv.Length, cipherBytes, 0, cipherBytes.Length);

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(request.key);
            aes.IV = iv;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream(cipherBytes);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);
            return streamReader.ReadToEnd();
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}