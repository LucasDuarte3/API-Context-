using Exo.WebApi.Models;
using Exo.WebApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Exo.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;

        public UsuariosController(UsuarioRepository usuarioRepository, IConfiguration configuration)
        {
            _usuarioRepository = usuarioRepository;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            return Ok(_usuarioRepository.Listar());
        }

        [HttpPost]
        public IActionResult Cadastrar(Usuario usuario)
        {
            _usuarioRepository.Cadastrar(usuario);
            return StatusCode(201);
        }

        [HttpGet("{id}")]
        public IActionResult BuscarPorID(int id)
        {
            Usuario usuario = _usuarioRepository.BuscarPorId(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return Ok(usuario);
        }

        [Authorize]
        [HttpPut("{id}")]
        public IActionResult Atualizar(int id, Usuario usuario)
        {
            _usuarioRepository.Atualizar(id, usuario);
            return StatusCode(204);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult Deletar(int id)
        {
            try
            {
                _usuarioRepository.Deletar(id);
                return StatusCode(204);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPost("login")]
        public IActionResult Login(LoginViewModel login)
        {
            Usuario usuario = _usuarioRepository.Login(login.Email, login.Senha);
            if (usuario == null)
            {
                return NotFound(new { mensagem = "Email ou senha inválidos!" });
            }
            
            // 🔹 CORREÇÃO AQUI: Usar a chave do appsettings.json
            var jwtKey = _configuration["Jwt:Key"] ?? throw new Exception("Chave JWT não configurada");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Jti, usuario.Id.ToString())
            };

            // 🔹 Verificação do tamanho (opcional, mas recomendado)
            if (Encoding.UTF8.GetBytes(jwtKey).Length < 32)
            {
                return BadRequest("Chave JWT muito curta. Necessário mínimo 32 bytes.");
            }

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 🔹 Monta o token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "Exo.WebApi",
                audience: _configuration["Jwt:Audience"] ?? "Exo.WebApi",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );
            
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }
    }
}