using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProjetoMetaMensagem.Servico.Auth
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GerarToken(string id, string email, string nome, string empresaId, string isAdmin)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            // Sem fallback: ver Program.cs para o motivo (chave hardcoded no codigo
            // anula a seguranca do JWT). Se chegou ate aqui o app ja teria falhado no
            // startup, mas mantemos a mesma checagem por seguranca.
            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JwtSettings:SecretKey nao configurado.");
            var issuer = jwtSettings["Issuer"] ?? "ProjetoMetaMensagem";
            var audience = jwtSettings["Audience"] ?? "ProjetoMetaMensagemApp";
            var expiryMinutes = int.Parse(jwtSettings["ExpiryInMinutes"] ?? "480");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Email, email ?? ""),
                new Claim(ClaimTypes.Name, nome),
                new Claim("empresaId", empresaId),
                new Claim("isAdmin", isAdmin ?? "false"),
                // Decidido no login, nao no request: quem dispensa o recorte por empresa e
                // so a conta de plataforma. Ver AdminsDaPlataforma para a diferenca dos dois niveis.
                new Claim("isAdminPlataforma", AdminsDaPlataforma.Contem(_configuration, email) ? "true" : "false")
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
