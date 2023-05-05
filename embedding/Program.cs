using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


class Program
{
    static async Task Main(string[] args)
    {
        // Set up the HttpClient with the OpenAI API endpoint and your API key
        var token="Bearer ";
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://api.openai.com/");
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer ");

        // Set up the request body as a JSON string
        string requestBody = @"{
            ""input"": ""Hi this is amit"",
            ""model"": ""text-embedding-ada-002""
        }";

        // Create the request content as a StringContent object
        var requestContent = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

        // Make the request to the embeddings endpoint
        HttpResponseMessage response = await httpClient.PostAsync("v1/embeddings", requestContent);

        // Get the response body as a string
        string responseBody = await response.Content.ReadAsStringAsync();
        JObject responseJson = JObject.Parse(responseBody);

        JToken embeddings = responseJson["data"][0]["embedding"];



        // Print the response body to the console
        Console.WriteLine(embeddings.Count());
    }
}
