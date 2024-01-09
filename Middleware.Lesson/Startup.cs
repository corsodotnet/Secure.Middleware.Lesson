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
                //Questo imposta lo schema di autenticazione predefinito per l'applicazione. Assegnando stai dicendo all'applicazione di utilizzare l'autenticazione JWT come meccanismo di autenticazione principale. Questo significa che, quando una richiesta arriva al server, il middleware di autenticazione prover� a convalidare il token JWT incluso nell'intestazione della richiesta.
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                //Questo imposta lo schema predefinito per le sfide di autenticazione. Quando un'azione richiede autenticazione e non � presente alcun token valido o l'autenticazione fallisce, il middleware genera una "sfida".
                //Impostando JwtBearerDefaults.AuthenticationScheme come schema di sfida predefinito, indichi che il middleware dovr� rispondere alle richieste non autenticate chiedendo un token JWT.
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    //Impostando questo valore su true, indichi che il middleware deve validare la chiave usata per firmare il token JWT in arrivo.
                    //Questo � un passaggio cruciale per garantire che il token sia stato emesso da una fonte attendibile e non sia stato manomesso.
                    ValidateIssuerSigningKey = true,

                    //Qui si specifica la chiave utilizzata per la validazione della firma del token.
                    //Questa chiave deve corrispondere a quella usata per firmare il token.
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("CreateSomeRandomStringForSecretKey")),

                    //Impostando questo valore su false, si specifica che il middleware non deve validare l'emittente del token.
                    //In alcuni casi, potresti volerlo validare per assicurarti che il token provenga da una fonte attendibile.
                    ValidateIssuer = false,

                    //Analogamente a ValidateIssuer, impostare questo valore su false indica che il middleware non deve
                    //validare l'audience (destinatario) del token. Anche questo pu� essere importante in scenari in cui devi assicurarti
                    //che il token sia destinato alla tua applicazione o a un particolare ascoltatore.
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
            app.UseMiddleware<AuthenticationMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();



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
    private readonly AppDbContext _appDbContext;

    public AuthenticationMiddleware(RequestDelegate next, AppDbContext appDbContext)
    {
        _next = next;
        _appDbContext = appDbContext;
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
        context.Request.EnableBuffering(); // Abilita la lettura del body pi� volte
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
            await _next(context); // Continua con la pipeline se l'utente � valido  
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



