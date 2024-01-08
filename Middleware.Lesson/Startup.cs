using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Middleware.Lesson.DB;
using System.Text;

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

            //  services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)

            //.AddCookie(options =>
            //{
            //    options.LoginPath = "/login"; // Imposta il tuo path di login
            //    options.LogoutPath = "/logout"; // Imposta il tuo path di logout
            //    options.Cookie.HttpOnly = true;  // Per la sicurezza
            //});

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
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("CreateSomeRandomStringForSecretKey")), // Usa una chiave segreta adeguata
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
            app.Use(async (context, next) =>
            {
                if (context.Request.Cookies.TryGetValue("AuthToken", out var token))
                {
                    context.Request.Headers.Add("Authorization", "Bearer " + token);
                }

                await next.Invoke();
            });
            app.UseAuthentication();
            app.UseAuthorization();



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }
    }

}




