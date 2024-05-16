using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Windows;



namespace ROSE_Online_Login_Manager.Resources.Util
{
    
    internal interface IPasswordEncryptor
    {
        /// <summary>
        ///     Encrypts a password stored in a <see cref="SecureString"/>.
        /// </summary>
        /// <param name="password">The <see cref="SecureString"/> containing the password to encrypt.</param>
        /// <returns>A byte array representing the encrypted password.</returns>
        byte[] Encrypt(SecureString password, byte[] iv);
    }






    /// <summary>
    ///     Provides encryption functionality for securing sensitive data.
    /// </summary>
    internal class AESEncryptor : IPasswordEncryptor
    {
        /// <summary>
        ///     Encrypts a <see cref="SecureString"/> using AES encryption with a user-defined or generated IV.
        /// </summary>
        /// <param name="password">The <see cref="SecureString"/> to encrypt.</param>
        /// <returns>The ciphertext representing the encrypted <see cref="SecureString"/> as a byte[].</returns>
        public byte[] Encrypt(SecureString password, byte[] iv)
        {
            byte[] encrypted;

            using (Aes aes = Aes.Create())
            {
                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aes.CreateEncryptor(HWIDGenerator.GetHWID(), iv);

                using MemoryStream memoryStream = new();
                using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
                using (StreamWriter streamWriter = new(cryptoStream))
                {
                    streamWriter.Write(SecureStringExtensions.ConvertSecureStringToString(password));
                }
                encrypted = memoryStream.ToArray();
            }

            return encrypted;
        }



        public static string Decrypt(byte[] cipherText)
        {
            string plaintext = string.Empty;

            using (Aes aes = Aes.Create())
            {
                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aes.CreateDecryptor(HWIDGenerator.GetHWID(), aes.IV);

                using MemoryStream ms = new(cipherText);
                using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
                using StreamReader reader = new(cs);
                plaintext = reader.ReadToEnd();
            }

            return plaintext;
        }
    }






    /// <summary>
    ///     Provides extension methods for <see cref="SecureString"/>.
    /// </summary>
    internal static class SecureStringExtensions
    {
        /// <summary>
        ///     Converts a <see cref="SecureString"/> to a byte array.
        /// </summary>
        /// <param name="secureString">The <see cref="SecureString"/> to convert.</param>
        /// <returns>A byte array representing the contents of the <see cref="SecureString"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="secureString"/> is null.</exception>
        public static string ConvertSecureStringToString(SecureString secureString)
        {
            ArgumentNullException.ThrowIfNull(secureString);

            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                if (valuePtr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
                }
            }
        }
    }
}
