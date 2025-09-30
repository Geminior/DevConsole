using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DevConsole.Infrastructure.Services
{
    public static class CryptoUtility
    {
        public static string Encrypt<T>(T data, string password) => Encrypt(SerializationUtil.SerializeCompact(data), password);

        public static string Encrypt(string data, string password) => Encrypt(Encoding.UTF8.GetBytes(data), password);

        public static string Encrypt(byte[] data, string password)
        {
            using var hasher = SHA256.Create();
            using var crypter = Aes.Create();
            crypter.KeySize = hasher.HashSize;
            crypter.Padding = PaddingMode.PKCS7;

            crypter.GenerateIV();
            crypter.Key = hasher.ComputeHash(Encoding.UTF8.GetBytes(password));
            using var encrypter = crypter.CreateEncryptor();
            using var memStream = new MemoryStream();
            memStream.Write(crypter.IV);
            memStream.Flush();

            using var cryptoStream = new CryptoStream(memStream, encrypter, CryptoStreamMode.Write);
            cryptoStream.Write(data);
            cryptoStream.FlushFinalBlock();

            return Convert.ToBase64String(memStream.ToArray());
        }

        public static T Decrypt<T>(byte[] data, string password) => Decrypt<T>(Encoding.UTF8.GetString(data), password);

        public static T Decrypt<T>(string data, string password) => SerializationUtil.DeserializeCompact<T>(Decrypt(data, password));

        public static string Decrypt(string data, string password)
        {
            var bytes = Convert.FromBase64String(data);

            using var hasher = SHA256.Create();
            using var crypter = Aes.Create();
            crypter.KeySize = hasher.HashSize;
            crypter.Padding = PaddingMode.PKCS7;
            var ivSize = crypter.BlockSize / 8;

            crypter.IV = bytes[..ivSize];
            crypter.Key = hasher.ComputeHash(Encoding.UTF8.GetBytes(password));

            using var encrypter = crypter.CreateDecryptor();
            using var memStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memStream, encrypter, CryptoStreamMode.Write);
            cryptoStream.Write(bytes[ivSize..]);
            cryptoStream.FlushFinalBlock();

            return Encoding.UTF8.GetString(memStream.ToArray());
        }
    }
}