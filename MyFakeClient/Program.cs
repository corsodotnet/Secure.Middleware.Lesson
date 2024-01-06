using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyFakeClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // URL dell'endpoint del server dove inviare la richiesta POST

            await SendGet();

            // Crea un'istanza di HttpClient

        }
        public static async Task SendGet()
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:5001");
                // Crea l'oggetto con le credenziali

                try
                {
                    // Invia la richiesta POST
                    HttpResponseMessage response = await client.GetAsync("/");

                    // Leggi la risposta
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Risposta ricevuta: " + result);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Errore nella richiesta: " + e.Message);
                }
            }
        }
        public async void SendPost(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                // Crea l'oggetto con le credenziali
                var credentials = new
                {
                    Username = "bruno", // Sostituire con l'username reale
                    Password = "myPassword"  // Sostituire con la password reale
                };

                // Serializza le credenziali in una stringa JSON
                string json = JsonSerializer.Serialize(credentials);
                StringContent data = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    // Invia la richiesta POST
                    HttpResponseMessage response = await client.PostAsync(url, data);

                    // Leggi la risposta
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
