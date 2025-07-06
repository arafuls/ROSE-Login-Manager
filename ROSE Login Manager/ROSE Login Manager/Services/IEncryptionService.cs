namespace ROSE_Login_Manager.Services;

/// <summary>
/// Service for encrypting and decrypting sensitive data using Windows DPAPI
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts data using Windows Data Protection API
    /// </summary>
    /// <param name="data">The data to encrypt</param>
    /// <returns>Encrypted data as bytes</returns>
    byte[] Encrypt(byte[] data);

    /// <summary>
    /// Encrypts a string using Windows Data Protection API
    /// </summary>
    /// <param name="text">The text to encrypt</param>
    /// <returns>Encrypted data as bytes</returns>
    byte[] EncryptString(string text);

    /// <summary>
    /// Decrypts data using Windows Data Protection API
    /// </summary>
    /// <param name="encryptedData">The encrypted data to decrypt</param>
    /// <returns>Decrypted data as bytes</returns>
    byte[] Decrypt(byte[] encryptedData);

    /// <summary>
    /// Decrypts data to a string using Windows Data Protection API
    /// </summary>
    /// <param name="encryptedData">The encrypted data to decrypt</param>
    /// <returns>Decrypted text</returns>
    string DecryptToString(byte[] encryptedData);
} 