using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Middleware.Lesson.DB;

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



            //In ASP.NET Core, il metodo services.AddAuthentication() configura il sistema di autenticazione dell'applicazione.
            //Quando lo configuri, puoi specificare diversi "schemi" di autenticazione che l'applicazione utilizzerà per gestire
            //l'identità degli utenti. I parametri chiave all'interno del metodo AddAuthentication includono:

            services.AddAuthentication(options =>
            {
                //DefaultAuthenticateScheme: Determina lo schema utilizzato per determinare
                //l'identità dell'utente per ogni richiesta.
                //In sostanza, è il meccanismo principale con cui il sistema verifica chi sei.
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                //DefaultSignInScheme: Determina lo schema utilizzato per la persistenza dell'identità dell'utente tra le
                //richieste,di solito tramite un cookie o un token.
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                //Determina lo schema utilizzato quando un utente accede a una risorsa che richiede l'autenticazione,
                //ma non è autenticato. Questo schema gestisce "come" l'applicazione sfida l'utente per l'autenticazione
                //- ad esempio, reindirizzandolo alla pagina di login di Google.
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;

                /*
                 Cosa Significano Questi Schemi?
                        [CookieAuthenticationDefaults.AuthenticationScheme]: Questo schema si basa sull'uso dei cookie. 
                Quando configuri DefaultAuthenticateScheme e DefaultSignInScheme con questo valore, stai dicendo 
                all'applicazione di utilizzare i cookie per mantenere la sessione dell'utente tra le richieste e per 
                autenticare l'utente su ciascuna richiesta. È comune nelle applicazioni web dove gli utenti tornano e
                rimangono autenticati tra diverse sessioni del browser.

                       [GoogleDefaults.AuthenticationScheme]: Questo schema è specifico per l'autenticazione di Google.
                Configurando DefaultChallengeScheme con questo valore, stai dicendo all'applicazione di reindirizzare 
                gli utenti non autenticati al servizio di autenticazione di Google quando tentano di accedere a risorse
                protette. È parte del flusso di autenticazione OAuth 2.0 di Google e viene usato per sfidare l'utente a 
                fornire le sue credenziali tramite il login di Google.
                 */


            })
            //.AddJwtBearer(options =>
            //{
            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuerSigningKey = true,
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("CreateSomeRandomStringForSecretKey")), // Imposta la chiave segreta
            //        ValidateIssuer = false,
            //        ValidateAudience = false
            //    };
            //});   
             .AddCookie(options =>
             {
                 options.LoginPath = "/google-login"; // Sostituisci con il tuo percorso di login
                 options.LogoutPath = "/logout"; // Sostituisci con il tuo percorso di logout
             })
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = "146162743362-p4k7fualc1jfglbh5lj8m5q238pdelih.apps.googleusercontent.com";
                googleOptions.ClientSecret = "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";
                googleOptions.CallbackPath = new PathString("/signin-google");
                // Puoi anche configurare altri parametri specifici di Google qui
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


}
