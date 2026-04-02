using System.Security.Cryptography;

namespace PrimeDiscipline.Application.Common;

public static class SessionToken
{
    public static string GenerateRawToken()
    {
        byte[] tokenBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(tokenBytes).ToLowerInvariant();
    }

    public static string HashToken(string token)
    {
        byte[] tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
        byte[] hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
