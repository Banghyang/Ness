using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace NessServer.Core
{
    public class Crypt
    {
        private const int MinPasswordLength = 12;
        private const int MaxPasswordLength = 128;
        private const int SaltSizeBytes = 32;
        private const int IVSizeBytes = 16;
        private const int KeySizeBytes = 32;
        private const int PBKDF2Iterations = 100_000;
        private readonly Action<string> _logAction;

        public Crypt()
        {
        }

        public byte[] EncryptMessage(string text, string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
            byte[] iv = RandomNumberGenerator.GetBytes(IVSizeBytes);

            byte[] dKey = DeriveKey(password, salt);

            byte[] data = Encoding.UTF8.GetBytes(text);
            byte[] encrypted = EncryptAes(data, dKey, iv);

            byte[] result = new byte[salt.Length + iv.Length + encrypted.Length];
            Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
            Buffer.BlockCopy(iv, 0, result, salt.Length, iv.Length);
            Buffer.BlockCopy(encrypted, 0, result, salt.Length + iv.Length, encrypted.Length);

            return result;
        }

        public string DecryptMessage(byte[] packet, string password)
        {
            byte[] salt = new byte[SaltSizeBytes];
            byte[] iv = new byte[IVSizeBytes];

            Buffer.BlockCopy(packet, 0, salt, 0, SaltSizeBytes);
            Buffer.BlockCopy(packet, SaltSizeBytes, iv, 0, IVSizeBytes);
            byte[] dKey = DeriveKey(password, salt);
            int encryptedLength = packet.Length - SaltSizeBytes - IVSizeBytes;
            byte[] encrypted = new byte[encryptedLength];
            Buffer.BlockCopy(packet, SaltSizeBytes + IVSizeBytes, encrypted, 0, encryptedLength);

            byte[] decrypted = DecryptAes(encrypted, dKey, iv);

            return Encoding.UTF8.GetString(decrypted);
        }

        public byte[] DeriveKey(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations: PBKDF2Iterations,
                hashAlgorithm: HashAlgorithmName.SHA256
            );

            return pbkdf2.GetBytes(KeySizeBytes);
        }

        static byte[] EncryptAes(byte[] data, byte[] Hkey, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Hkey;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                    return encryptor.TransformFinalBlock(data, 0, data.Length);
            }
        }

        public string Decrypt(byte[] encryptedData, string password, byte[] salt, byte[] iv)
        {
            byte[] dKey = DeriveKey(password, salt);
            byte[] decrypted = DecryptAes(encryptedData, dKey, iv);
            return Encoding.UTF8.GetString(decrypted);
        }

        static byte[] DecryptAes(byte[] encryptedData, byte[] Hkey, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Hkey;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor())
                    return decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            }
        }

    }
}