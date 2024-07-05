using NLog;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;



namespace ROSE_Login_Manager.Services
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
        ///     Encrypts the specified password using the AES algorithm.
        /// </summary>
        /// <param name="password">The password to encrypt.</param>
        /// <param name="iv">The initialization vector (IV) used in encryption.</param>
        /// <returns>The encrypted bytes representing the password.</returns>
        public byte[] Encrypt(SecureString password, byte[] iv)
        {
            try
            {
                byte[] encrypted;

                // Create a new instance of the AES algorithm.
                using (Aes aes = Aes.Create())
                {
                    aes.BlockSize = 128; // Ensure that the block size matches the IV size (16 bytes).

                    // Create an encryptor to perform the stream transform.
                    ICryptoTransform encryptor = aes.CreateEncryptor(HWIDGenerator.GetHWID(), iv);

                    // Encrypt the password.
                    using MemoryStream memoryStream = new();
                    using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
                    using (StreamWriter streamWriter = new(cryptoStream))
                    {
                        // Convert SecureString to string and write it to the stream.
                        streamWriter.Write(SecureStringExtensions.ConvertSecureStringToString(password));
                    }
                    encrypted = memoryStream.ToArray();
                }

                return encrypted;
            }
            catch (CryptographicException ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
                throw;
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
                throw;
            }
        }



        /// <summary>
        ///     Decrypts the specified cipher text using the AES algorithm.
        /// </summary>
        /// <param name="cipherText">The cipher text to decrypt.</param>
        /// <param name="iv">The initialization vector (IV) used in the decryption process.</param>
        /// <returns>The decrypted plain text string.</returns>
        public static string Decrypt(byte[] cipherText, byte[] iv)
        {
            try
            {
                string plaintext = string.Empty;

                // Create a new instance of the AES algorithm.
                using (Aes aes = Aes.Create())
                {
                    // Ensure that the block size matches the IV size (16 bytes).
                    aes.BlockSize = 128;

                    // Create a decryptor to perform the stream transform.
                    ICryptoTransform decryptor = aes.CreateDecryptor(HWIDGenerator.GetHWID(), iv);

                    // Decrypt the cipher text.
                    using MemoryStream memoryStream = new(cipherText);
                    using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
                    using StreamReader reader = new(cryptoStream);
                    plaintext = reader.ReadToEnd();
                }

                return plaintext;
            }
            catch (CryptographicException ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
                throw;
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
                throw;
            }
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

            nint valuePtr = nint.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                if (valuePtr != nint.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
                }
            }
        }
    }
}
