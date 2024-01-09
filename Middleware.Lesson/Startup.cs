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

            #region Configura l'autenticazione JWT


            services.AddAuthentication(options =>
            {
                //Questo imposta lo schema di autenticazione predefinito per l'applicazione.
                //Assegnando stai dicendo all'applicazione di utilizzare l'autenticazione JWT come meccanismo di autenticazione principale.
                //Questo significa che, quando una richiesta arriva al server, il middleware di autenticazione proverà a convalidare
                //il token JWT incluso nell'intestazione della richiesta.
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                //Questo imposta lo schema predefinito per le sfide di autenticazione.
                //Quando un'azione richiede autenticazione e non è presente alcun token valido o l'autenticazione fallisce,
                //il middleware genera una "sfida".
                //Impostando JwtBearerDefaults.AuthenticationScheme come schema di sfida predefinito,
                //indichi che il middleware dovrà rispondere alle richieste non autenticate chiedendo un token JWT.
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    //Impostando questo valore su true, indichi che il middleware deve validare la chiave usata per firmare il token JWT in arrivo.
                    //Questo è un passaggio cruciale per garantire che il token sia stato emesso da una fonte attendibile e non sia stato manomesso.
                    ValidateIssuerSigningKey = true,


                    //Qui si specifica la chiave utilizzata per la validazione della firma del token.
                    //Questa chiave deve corrispondere a quella usata per firmare il token.
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("CreateSomeRandomStringForSecretKey")),


                    //Questa impostazione determina se il middleware deve validare il campo iss (issuer, cioè l'emittente) del token JWT.
                    //Impostando questo valore su false, si specifica che il middleware non deve validare l'emittente del token.
                    //In alcuni casi, potresti volerlo validare per assicurarti che il token provenga da una fonte attendibile.
                    ValidateIssuer = false,

                    // Indica che il middleware non controllerà se il token è destinato specificamente all'applicazione in questione.
                    //Questo può essere appropriato in ambienti in cui il token è condiviso tra più applicazioni o servizi.
                    ValidateAudience = false
                };
            });


            #endregion         

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

            //   app.UseMiddleware<AuthenticationMiddleware>();

            #region  Authc/Autrz
            app.UseAuthentication();
            app.UseAuthorization();
            #endregion

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }
    }

}
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;


    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;

    }
    public class CredentialsDto
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
        CredentialsDto credentials = null;
        try
        {
            credentials = JsonSerializer.Deserialize<CredentialsDto>(body);
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

    private bool IsValidUser(CredentialsDto credentials)
    {
        // Implementa la logica di verifica dell'utente
        return credentials.Username == "bruno" && credentials.Password == "myPassword";
    }
}



