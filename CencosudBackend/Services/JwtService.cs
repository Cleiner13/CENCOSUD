using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CencosudBackend.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CencosudBackend.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerarToken(LoginResult usuario)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key not found");
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var expiresHours = int.Parse(jwtSection["ExpiresHours"] ?? "3");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario?.ToString() ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Usuario ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario?.ToString() ?? string.Empty),
                new Claim(ClaimTypes.Role, usuario.RolApp ?? string.Empty),
                new Claim("cargo", usuario.Cargo ?? string.Empty),
                new Claim("supervisor", usuario.Supervisor ?? string.Empty),
                new Claim("uunn", usuario.UUNN ?? string.Empty)
            };

            var keyBytes = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(keyBytes, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresHours * 60),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
