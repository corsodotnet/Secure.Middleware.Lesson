using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Middleware.Lesson.DB;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Middleware.Lesson
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            // Configura il database in-memory
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName: "UserDatabase"));

            // Configura l'autenticazione JWT
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("CreateSomeRandomStringForSecretKey")), // Imposta la chiave segreta
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }
    }
    public class MyMiddleware
    {
        private readonly RequestDelegate _next;

        public MyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Logica del middleware prima di passare alla prossima componente

            await context.Response.WriteAsync("MyMiddleware - MyMiddleware - Andata \n");


            // Passa al prossimo middleware nella pipeline 

            await _next(context);
            await context.Response.WriteAsync("MyMiddleware - MyMiddleware - Ritorno \n");

            // Logica da eseguire dopo che il prossimo middleware ha terminato
            // ...
        }
    }
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public class Credentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
        public async Task InvokeAsync(HttpContext context)
        {
            // Leggi il body della richiesta
            string body;
            context.Request.EnableBuffering(); // Abilita la lettura del body più volte
            using (var reader = new StreamReader(context.Request.Body))
            {
                body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; // Resetta la posizione del body per le letture successive
            }

            // Deserializza il body nella classe Credentials
            Credentials credentials = null;
            try
            {
                credentials = JsonSerializer.Deserialize<Credentials>(body);
            }
            catch (JsonException exx) // Gestisci errori di deserializzazione
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync("Body della richiesta non valido.");
                return;
            }

            // Verifica le credenziali
            if (credentials != null && IsValidUser(credentials))
            {
                await _next(context); // Continua con la pipeline se l'utente è valido  
                await context.Response.WriteAsync("Accesso Concesso!");
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Accesso Negato");
            }
        }

        private bool IsValidUser(Credentials credentials)
        {
            // Implementa la logica di verifica dell'utente
            return credentials.Username == "bruno" && credentials.Password == "myPassword";
        }
    }


}
