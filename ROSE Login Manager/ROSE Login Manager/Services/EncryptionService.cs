using System.Security.Cryptography;
using System.Text;

namespace ROSE_Login_Manager.Services;

/// <summary>
/// Implementation of encryption service using Windows Data Protection API (DPAPI)
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _entropy;

    public EncryptionService()
    {
        // Create entropy specific to this application to strengthen encryption
        _entropy = Encoding.UTF8.GetBytes("ROSE-Login-Manager-v1.0");
    }

    /// <summary>
    /// Encrypts data using Windows Data Protection API
    /// </summary>
    /// <param name="data">The data to encrypt</param>
    /// <returns>Encrypted data as bytes</returns>
    public byte[] Encrypt(byte[] data)
    {
        if (data == null || data.Length == 0)
            return Array.Empty<byte>();

        try
        {
            return ProtectedData.Protect(data, _entropy, DataProtectionScope.CurrentUser);
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException("Failed to encrypt data", ex);
        }
    }

    /// <summary>
    /// Encrypts a string using Windows Data Protection API
    /// </summary>
    /// <param name="text">The text to encrypt</param>
    /// <returns>Encrypted data as bytes</returns>
    public byte[] EncryptString(string text)
    {
        if (string.IsNullOrEmpty(text))
            return Array.Empty<byte>();

        var data = Encoding.UTF8.GetBytes(text);
        return Encrypt(data);
    }

    /// <summary>
    /// Decrypts data using Windows Data Protection API
    /// </summary>
    /// <param name="encryptedData">The encrypted data to decrypt</param>
    /// <returns>Decrypted data as bytes</returns>
    public byte[] Decrypt(byte[] encryptedData)
    {
        if (encryptedData == null || encryptedData.Length == 0)
            return Array.Empty<byte>();

        try
        {
            return ProtectedData.Unprotect(encryptedData, _entropy, DataProtectionScope.CurrentUser);
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException("Failed to decrypt data", ex);
        }
    }

    /// <summary>
    /// Decrypts data to a string using Windows Data Protection API
    /// </summary>
    /// <param name="encryptedData">The encrypted data to decrypt</param>
    /// <returns>Decrypted text</returns>
    public string DecryptToString(byte[] encryptedData)
    {
        if (encryptedData == null || encryptedData.Length == 0)
            return string.Empty;

        var decryptedData = Decrypt(encryptedData);
        return Encoding.UTF8.GetString(decryptedData);
    }
} 