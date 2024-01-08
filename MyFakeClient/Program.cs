using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyFakeClient
{
    internal class Program
    {
        private static string _token; // Simulate cache 

        static async Task Main(string[] args)
        {
            // Registrazione
            await SendPost("https://localhost:5001/api/auth/register", new
            {
                Username = "nuovoUtente",
                Password = "nuovaPassword"
            });

            // Utilizzo del token per una richiesta a un endpoint protetto
            await SendGetWithToken("https://localhost:5001/WeatherForecast");
        }

        public static async Task SendPost(string url, object credentials)
        {
            using (HttpClient client = new HttpClient())
            {
                string json = JsonSerializer.Serialize(credentials);
                StringContent data = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.PostAsync(url, data);
                    string result = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        _token = result; // Salva il token
                    }

                    Console.WriteLine("Risposta ricevuta: " + result);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Errore nella richiesta: " + e.Message);
                }
            }
        }

        public static async Task SendGetWithToken(string url)
        {
            Console.WriteLine(url);
            Console.WriteLine(_token);
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                    HttpResponseMessage response = await client.GetAsync(url);
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Risposta ricevuta: " + result);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Errore nella richiesta: " + e.Message);
                }
            }
        }
    }
}
