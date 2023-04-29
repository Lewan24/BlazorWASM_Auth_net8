using System.Security.Cryptography;
using System.Text;

namespace WASMWithAuth.Server.Data
{
    public static class EncryptorDecryptor
    {
        public static string Encrypt(string plainText, string key)
        {
            byte[] iv = new byte[16];
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.GenerateIV();
                iv = aes.IV;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }
                    }
                    byte[] cipherText = memoryStream.ToArray();
                    byte[] resultBytes = new byte[iv.Length + cipherText.Length];
                    Buffer.BlockCopy(iv, 0, resultBytes, 0, iv.Length);
                    Buffer.BlockCopy(cipherText, 0, resultBytes, iv.Length, cipherText.Length);
                    return Convert.ToBase64String(resultBytes);
                }
            }
        }

        public static string Decrypt(string cipherText, string key)
        {
            try
            {
                byte[] iv = new byte[16];
                byte[] buffer = Convert.FromBase64String(cipherText);

                Buffer.BlockCopy(buffer, 0, iv, 0, iv.Length);

                byte[] cipherBytes = new byte[buffer.Length - iv.Length];
                Buffer.BlockCopy(buffer, iv.Length, cipherBytes, 0, cipherBytes.Length);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key);
                    aes.IV = iv;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream(cipherBytes))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader(cryptoStream))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }

}
