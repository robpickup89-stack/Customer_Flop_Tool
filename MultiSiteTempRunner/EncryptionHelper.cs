using System.Security.Cryptography;

namespace MultiSiteTempRunner;

/// <summary>
/// Provides AES-256 encryption and decryption functionality with PBKDF2 key derivation.
/// </summary>
public static class EncryptionHelper
{
    private const int SaltSize = 16;
    private const int IvSize = 16;
    private const int KeySize = 32; // 256 bits
    private const int Iterations = 100000;

    /// <summary>
    /// Encrypts data using AES-256 with a password-derived key.
    /// </summary>
    /// <param name="plainData">The data to encrypt.</param>
    /// <param name="password">The password for encryption.</param>
    /// <returns>Encrypted data with salt and IV prepended.</returns>
    public static byte[] Encrypt(byte[] plainData, string password)
    {
        if (plainData == null || plainData.Length == 0)
            throw new ArgumentException("Data cannot be null or empty.", nameof(plainData));
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        // Generate random salt and IV
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] iv = RandomNumberGenerator.GetBytes(IvSize);

        // Derive key using PBKDF2
        byte[] key = DeriveKey(password, salt);

        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = iv;

        using var encryptor = aes.CreateEncryptor();
        byte[] encryptedData = encryptor.TransformFinalBlock(plainData, 0, plainData.Length);

        // Combine salt + IV + encrypted data
        byte[] result = new byte[SaltSize + IvSize + encryptedData.Length];
        Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
        Buffer.BlockCopy(iv, 0, result, SaltSize, IvSize);
        Buffer.BlockCopy(encryptedData, 0, result, SaltSize + IvSize, encryptedData.Length);

        return result;
    }

    /// <summary>
    /// Decrypts data that was encrypted using the Encrypt method.
    /// </summary>
    /// <param name="encryptedData">The encrypted data with salt and IV prepended.</param>
    /// <param name="password">The password used for encryption.</param>
    /// <returns>The decrypted data.</returns>
    public static byte[] Decrypt(byte[] encryptedData, string password)
    {
        if (encryptedData == null || encryptedData.Length < SaltSize + IvSize + 1)
            throw new ArgumentException("Invalid encrypted data.", nameof(encryptedData));
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        // Extract salt, IV, and cipher text
        byte[] salt = new byte[SaltSize];
        byte[] iv = new byte[IvSize];
        byte[] cipherText = new byte[encryptedData.Length - SaltSize - IvSize];

        Buffer.BlockCopy(encryptedData, 0, salt, 0, SaltSize);
        Buffer.BlockCopy(encryptedData, SaltSize, iv, 0, IvSize);
        Buffer.BlockCopy(encryptedData, SaltSize + IvSize, cipherText, 0, cipherText.Length);

        // Derive key using PBKDF2
        byte[] key = DeriveKey(password, salt);

        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
    }

    /// <summary>
    /// Encrypts a zip file and saves it with an encrypted extension.
    /// </summary>
    /// <param name="zipData">The zip file data to encrypt.</param>
    /// <param name="outputPath">The output file path.</param>
    /// <param name="password">The password for encryption.</param>
    public static void EncryptZipToFile(byte[] zipData, string outputPath, string password)
    {
        byte[] encryptedData = Encrypt(zipData, password);
        File.WriteAllBytes(outputPath, encryptedData);
    }

    /// <summary>
    /// Decrypts an encrypted zip file.
    /// </summary>
    /// <param name="encryptedFilePath">The path to the encrypted file.</param>
    /// <param name="password">The password for decryption.</param>
    /// <returns>The decrypted zip data.</returns>
    public static byte[] DecryptZipFromFile(string encryptedFilePath, string password)
    {
        byte[] encryptedData = File.ReadAllBytes(encryptedFilePath);
        return Decrypt(encryptedData, password);
    }

    /// <summary>
    /// Checks if a file is an encrypted zip based on extension.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns>True if the file has an encrypted zip extension.</returns>
    public static bool IsEncryptedZip(string filePath)
    {
        string ext = Path.GetExtension(filePath).ToLowerInvariant();
        string fullName = Path.GetFileName(filePath).ToLowerInvariant();
        return ext == ".enczip" || fullName.EndsWith(".zip.enc");
    }

    private static byte[] DeriveKey(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(KeySize);
    }
}
