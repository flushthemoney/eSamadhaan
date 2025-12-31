using eSamadhaan.Application.Interfaces.Services;
using System.Security.Cryptography;
using System.Text;

namespace eSamadhaan.Application.Services;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        using var hmac = new HMACSHA512();
        var salt = hmac.Key;
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        
        // Combine salt and hash
        var hashBytes = new byte[salt.Length + hash.Length];
        Array.Copy(salt, 0, hashBytes, 0, salt.Length);
        Array.Copy(hash, 0, hashBytes, salt.Length, hash.Length);
        
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        var hashBytes = Convert.FromBase64String(storedHash);
        
        // Extract salt (first 128 bytes for HMACSHA512)
        var salt = new byte[128];
        Array.Copy(hashBytes, 0, salt, 0, 128);
        
        using var hmac = new HMACSHA512(salt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        
        // Compare computed hash with stored hash
        for (int i = 0; i < computedHash.Length; i++)
        {
            if (hashBytes[i + 128] != computedHash[i])
            {
                return false;
            }
        }
        
        return true;
    }
}
