using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    private static readonly HttpClient sharedClient = new()
    {
        BaseAddress = new Uri("http://api.weatherapi.com/v1/"),
    };

    static async Task<string> GetAsync(HttpClient httpClient, string location)
    {
        using HttpResponseMessage response = await httpClient.GetAsync($"current.json?key=&q={location}&aqi=no");

        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();

        using JsonDocument doc = JsonDocument.Parse(jsonResponse);
        JsonElement root = doc.RootElement;

        // Access the specific parts of the JSON
        double temperature = root.GetProperty("current").GetProperty("temp_c").GetDouble();
        string condition = root.GetProperty("current").GetProperty("condition").GetProperty("text").GetString();

        return $"({temperature}°C, {condition})";
    }

    static async Task<string> AddWeatherData(HttpClient client, string text)
    {
        StringBuilder builder = new StringBuilder();

        string[] lines = text.Split("\r\n");

        string line = lines[0];
        string[] citites = line.Split(", ");

        string[] weatherData = new string[citites.Length];

        for (int i = 0; i < citites.Length; i++)
        {
            weatherData[i] = await GetAsync(client, citites[i]);
        }

        for (int i = 1; i < 6; i++)
        {
            if(i > 1) builder.Append("\r\n");

            string currLine = lines[i];
            currLine = currLine.Replace(citites[i - 1], citites[i - 1] + " " + weatherData[i - 1]);

            builder.Append(currLine);
        }

        return builder.ToString();
    }

    static async Task Main(string[] args)
    {
        string text = "Malaga, Alicante, Nice, Santorini, Dubrovnik\r\n- Malaga offers sunny beaches, historic sites, and vibrant nightlife.\r\n- Alicante boasts beautiful beaches, a charming Old Town, and nearby mountains.\r\n- Nice combines French elegance with Mediterranean beaches and warm weather.\r\n- Santorini features iconic blue-domed churches, stunning sunsets, and volcanic beaches.\r\n- Dubrovnik offers a well-preserved medieval Old Town, crystal-clear waters, and beautiful beaches.\r\nEND";

        Console.WriteLine(await AddWeatherData(sharedClient, text));
    }
}
