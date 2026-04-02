using Konscious.Security.Cryptography;
using PrimeDiscipline.Application.Interfaces;
using System.Security.Cryptography;

namespace PrimeDiscipline.Infrastructure.Security;

/// <summary>
/// Argon2id password hasher.
/// Parameters: 4 iterations, 64 MB memory, 2 parallel lanes — balanced for server use.
/// Output format: base64(salt):base64(hash)
/// </summary>
public sealed class Argon2PasswordHasher : IPasswordHasher
{
    private const int SaltSize       = 16;  // bytes
    private const int HashSize        = 32;  // bytes
    private const int Iterations      = 4;
    private const int MemorySizeKb    = 65536; // 64 MB
    private const int DegreeOfParallelism = 2;

    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        byte[] hash = ComputeHash(password, salt);

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string storedHash)
    {
        string[] parts = storedHash.Split(':');
        if (parts.Length != 2) return false;

        byte[] salt;
        byte[] expectedHash;
        try
        {
            salt         = Convert.FromBase64String(parts[0]);
            expectedHash = Convert.FromBase64String(parts[1]);
        }
        catch (FormatException)
        {
            return false;
        }

        byte[] actualHash = ComputeHash(password, salt);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static byte[] ComputeHash(string password, byte[] salt)
    {
        using Argon2id argon2 = new(System.Text.Encoding.UTF8.GetBytes(password))
        {
            Salt                 = salt,
            Iterations           = Iterations,
            MemorySize           = MemorySizeKb,
            DegreeOfParallelism  = DegreeOfParallelism,
        };

        return argon2.GetBytes(HashSize);
    }
}
