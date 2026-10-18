// Felipe Antonio Brüggemann
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace AcademiaDaDuda.Application.Security;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int DefaultMemorySizeKb = 64 * 1024; // 64 MB
    private const int DefaultIterations = 3;
    private const int MaximumDegreeOfParallelism = 64;
    private const int MaximumPasswordLength = 1024;

    public static string Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));
        if (password.Length > MaximumPasswordLength)
            throw new ArgumentException("Password is too long.", nameof(password));

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        int p = Math.Clamp(Environment.ProcessorCount, 1, MaximumDegreeOfParallelism);

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = p,
            MemorySize = DefaultMemorySizeKb,
            Iterations = DefaultIterations
        };

        byte[] hash = argon2.GetBytes(HashSize);

        return $"ARGON2ID:{DefaultIterations}:{DefaultMemorySizeKb}:{p}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password)
            || password.Length > MaximumPasswordLength
            || string.IsNullOrWhiteSpace(passwordHash)
            || passwordHash.Length > 256)
            return false;

        var parts = passwordHash.Split(':');
        if (parts.Length != 6) return false;
        if (!parts[0].Equals("ARGON2ID", StringComparison.Ordinal)) return false;
        if (!int.TryParse(parts[1], out int iterations) || iterations != DefaultIterations) return false;
        if (!int.TryParse(parts[2], out int memorySizeKb) || memorySizeKb != DefaultMemorySizeKb) return false;
        if (!int.TryParse(parts[3], out int degreeOfParallelism)
            || degreeOfParallelism is < 1 or > MaximumDegreeOfParallelism)
            return false;

        byte[] salt;
        byte[] expected;

        try
        {
            salt = Convert.FromBase64String(parts[4]);
            expected = Convert.FromBase64String(parts[5]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (salt.Length != SaltSize || expected.Length != HashSize)
            return false;

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = degreeOfParallelism,
            MemorySize = memorySizeKb,
            Iterations = iterations
        };

        byte[] actual = argon2.GetBytes(HashSize);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
