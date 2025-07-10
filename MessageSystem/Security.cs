using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using EF_Messages; // Adjust namespace as needed

public static class SecurityService
{
    private const string SecretKey = "your_super_secret_key_12345";
    private const string Issuer = "your-app";
    private const string Audience = "your-api";

    public static string? Login(string username, string password, MessageSystemContext db)
    {
        // Validate user credentials against the database
        var user = db.Users.FirstOrDefault(u => u.Name == username /*&& u.Password == password*/);
        if (user == null)
            return null;

        // Create claims
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Create signing credentials
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Create the JWT token
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}