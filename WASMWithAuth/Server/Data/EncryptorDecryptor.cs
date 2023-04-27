using System.Security.Cryptography;
using System.Text;

namespace WASMWithAuth.Server.Data
{
    public static class EncryptorDecryptor
    {
        public static string Encrypt(string plainText, string key)
        {
            // generujemy losowy wektor inicjujący
            byte[] iv = new byte[16];
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.GenerateIV(); // generujemy losowy wektor inicjujący
                iv = aes.IV;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            // zapisujemy tekst do strumienia
                            streamWriter.Write(plainText);
                        }
                    }
                    // zwracamy zaszyfrowany tekst razem z wektorem inicjującym
                    return Convert.ToBase64String(iv.Concat(memoryStream.ToArray()).ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText, string key)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

    }
}
