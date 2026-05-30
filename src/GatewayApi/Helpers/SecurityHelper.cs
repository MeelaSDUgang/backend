using System.Security.Cryptography;
using System.Text;

namespace GatewayApi.Helpers;

public static class SecurityHelper
{
    public static string GenerateSecretKey(int length = 32)
    {
        var randomBytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }

    public static string ComputeHmacSignature(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var messageBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);

        return Convert.ToHexString(hashBytes).ToLower();
    }
}