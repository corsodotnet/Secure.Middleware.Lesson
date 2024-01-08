using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MyFakeClient
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static string baseAddress = "https://localhost:5001/"; // Assicurati che corrisponda all'URL del tuo server

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Request without Login...");
            try
            {
                await GetSecureData();

            }
            catch (Exception)
            {

                throw;
            }
            // Registrazione (se necessario)
            await RegisterUser("testuser", "testpassword");

            // Login
            await LoginUser("testuser", "testpassword");

            // Effettua una richiesta autenticata
            await GetSecureData();
        }

        private static async Task RegisterUser(string username, string password)
        {
            var url = baseAddress + "api/auth/register";
            var user = new { Username = username, Password = password };
            var json = JsonConvert.SerializeObject(user);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, data);
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Registration: " + result);
        }

        private static async Task LoginUser(string username, string password)
        {
            var url = baseAddress + "api/auth/login";
            var user = new { Username = username, Password = password };
            var json = JsonConvert.SerializeObject(user);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, data);
            var result = await response.Content.ReadAsStringAsync();

            // Salva il cookie per le successive richieste
            if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                client.DefaultRequestHeaders.Add("Cookie", string.Join(";", cookies));
            }

            Console.WriteLine("Login: " + result);
        }

        private static async Task GetSecureData()
        {
            var url = baseAddress + "WeatherForecast"; // Modifica con il percorso corretto
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                Console.WriteLine("WeatherForecast Data: " + data);
            }
            else
            {
                Console.WriteLine("Error accessing WeatherForecast data. Status Code: " + response.StatusCode);
            }
        }
    }

}
