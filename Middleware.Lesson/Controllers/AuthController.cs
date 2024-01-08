﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Lesson.Models
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DB.AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(DB.AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto userDto)
        {
            // Verify if the username already exists
            var userExists = await _context.Users.AnyAsync(u => u.Username == userDto.Username);
            if (userExists)
            {
                return BadRequest("Username already exists.");
            }

            CreatePasswordHash(userDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Username = userDto.Username,
                PasswordHash = passwordHash, // Store the hash
                PasswordSalt = passwordSalt  // Store the salt
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] UserDto request)
        {

            var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Unauthorized("Username or password is incorrect.");
            }

            var token = GenerateJwtToken(request.Username); // Genera il token JWT

            // Imposta il token in un cookie sicuro
            var cookieOptions = new CookieOptions
            {
                //Un cookie HttpOnly è un tipo di cookie che è inaccessibile tramite script lato client, come JavaScript. Questo significa che anche se un attaccante dovesse riuscire a eseguire uno script cross-site (XSS) sulla tua pagina, non sarebbe in grado di leggere il cookie HttpOnly.
                HttpOnly = true,
                Secure = true //Un cookie Secure è un cookie che viene trasmesso solo su connessioni criptate HTTPS. Se un cookie ha l'attributo Secure, non verrà inviato dal client se la connessione non è HTTPS.
            };
            Response.Cookies.Append("AuthToken", token, cookieOptions);

            return Ok(new { Token = token });
        }


        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key; // Save the generated salt
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt); // Use the stored salt
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(storedHash); // Compare the computed hash with the stored hash
        }


        private string GenerateJwtToken(string user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("CreateSomeRandomStringForSecretKey"); // Retrieve the secret key securely
            var tokenDescriptor = new SecurityTokenDescriptor
            {

                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),

                // SigningCredentials: Questa è una classe nel namespace Microsoft.IdentityModel.Tokens.Viene utilizzata per definire la chiave crittografica e l'algoritmo di sicurezza che saranno usati per generare la firma del JWT.
                // SecurityAlgorithms.HmacSha512Signature: Questo specifica l'algoritmo utilizzato per generare la firma. HMAC SHA-512 è una funzione hash crittografica che garantisce l'integrità e l'autenticità del token. Utilizzare HMAC SHA-512 significa che la parte della firma del JWT viene generata utilizzando questo algoritmo, noto per la sua forza crittografica.
                // SymmetricSecurityKey: Questa classe rappresenta una chiave di crittografia simmetrica. Nel tuo codice, viene istanziata con key, che dovrebbe essere un array di byte.Questa chiave viene utilizzata sia per firmare che per convalidare il token.È importante che questa chiave sia mantenuta sicura e non esposta o codificata in modo fisso nel tuo codice.Spesso viene conservata in un archivio di configurazione sicuro o nelle variabili di ambiente.
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
