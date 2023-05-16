using System.ComponentModel;
using System.Net;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;


internal class Program

{
    private static async  Task Main(string[] args)
    {
        string url="https://www.cloudcraftz.com/";
        int level=3;
        bool Flags=true;

        List<string> web_url = new List<string>();
        web_url.Add(url);


        var all_links=FilteredLink(web_url,level,Flags);

        Console.WriteLine("Number of links to Scrapes "+ all_links.Count);
        Console.WriteLine(String.Join(Environment.NewLine, all_links));
        Console.WriteLine("\n");


        await Linktotext(all_links);
       
    }

    static List<String>FilteredLink (List<string>web_url,int level,bool Flags)
    {
        List<string>local_url=new List<string>();
        local_url.AddRange(web_url);
        while(level!=0)
        {
            foreach (var items in local_url.ToList())
            {
                var docs= GetDocument(items);
                var new_links=ExtractLinks(docs,Flags);
                foreach (var links in new_links)
                {
                    if(!web_url.Contains(links))
                    {
                        web_url.Add(links);
                        local_url.Add(links);
                    }
                    local_url.Remove(links);                  
                }
            }
        level--;
        }
        return web_url; 
    }

    static HtmlDocument GetDocument(string url)
    {
        HtmlWeb web = new HtmlWeb();
        
        HtmlDocument doc = web.Load(url);
     
        return doc;
    }
    
   

    static string Gettext(HtmlDocument doc)
    {
        string str=doc.DocumentNode.InnerText.Trim();
        string cleanedText = Regex.Replace(str, @"\s+", " ");
        string outputtext = Regex.Replace(cleanedText, @"#\d+", "");
        string cleanedtext = Regex.Replace(outputtext, @"[^0-9a-zA-Z]+", " ").Trim();
        return cleanedtext ;
    }
    
    static List<string> ExtractLinks(HtmlDocument doc,bool Flags)
    {
        List<string> hrefTags = new List<string>();
        foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
        {
            HtmlAttribute att = link.Attributes["href"];
            if (att!=null)
            {
                hrefTags.Add(att.Value);
            }         
        }


        List<string> httpLinks = hrefTags.Where(link => link.StartsWith("http")).ToList();

        if (Flags==true){
            List<string> filteredList = httpLinks.Where(s => s.Contains("www.cloudcraftz")).ToList();
            return filteredList; 

        }

        // List<string> filteredList = hrefTags.Where(s => s.Contains("www.cloudcraftz")).ToList();


        else{
            return httpLinks;
        }  
    }

    static async Task Linktotext(List<String>links)
    {
        JArray embeddingVector = new JArray();
        foreach(var items in links)
        {
            var doc= GetDocument(items);
            var str=Gettext(doc);
            
            JToken embeddings = await texttoemb(str);
            JArray embeddingsArray = new JArray(embeddings);
            embeddingVector.Add(embeddingsArray);
            // Console.Write("embedding generated\n");

            string[] substrings = items.Split('/');
            string elem=substrings [substrings.Length-2];
            string txt=".txt";
            string file_name=elem +txt;

            var status = await UpsertVector(elem, str,embeddingsArray);

            if (status == HttpStatusCode.OK)
            {
                Console.WriteLine("Embedding upserted successfully.");
            }
            else
            {
                Console.WriteLine("Error during upserting embedding:");
                Console.WriteLine(status);
            }

            using (StreamWriter writer = new StreamWriter(file_name))
            {
                writer.WriteLine(str);
            }

            Console.WriteLine("Text saved to file: " + elem);
            Console.WriteLine("\n");
        }

        string json = JsonConvert.SerializeObject(embeddingVector);

// Write the JSON string to a file
        File.WriteAllText(@"vector.json", json);
     
        // Console.WriteLine(embeddingVector.ToString());   
    }

    static async Task<JToken>texttoemb (string text)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://api.openai.com/");
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer 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");

        // Set up the request body as a JSON string
        string requestBody = @"{
            ""input"": """+ text + @""",
            ""model"": ""text-embedding-ada-002""
        }";

        // Create the request content as a StringContent object
        var requestContent = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

        // Make the request to the embeddings endpoint
        HttpResponseMessage response = await httpClient.PostAsync("v1/embeddings", requestContent);

        // Get the response body as a string
        try 
        {
            // Get the response body as a string
            string responseBody = await response.Content.ReadAsStringAsync();
            
            // Parse the response JSON
            JObject responseJson = JObject.Parse(responseBody);

            // Console.WriteLine(responseJson);

            // Extract the embeddings data
            JToken embeddings = responseJson["data"][0]["embedding"];
            
            // Console.WriteLine(embeddings);

            return embeddings;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return null;
        }
    }

    static async Task<HttpStatusCode> UpsertVector(string id, string metadataText,JArray embvetcor )
    {
    // Set up the request payload
    var payload = new
    {
        vectors = new[]
        {
            new
            {
                id = id,
                values = embvetcor,
                metadata = new
                {
                    text = metadataText
                }
            }
        }
    };

    // Serialize the payload to a JSON string
    var json = JsonConvert.SerializeObject(payload);

    // Set up the HTTP request
    var request = new HttpRequestMessage
    {
        Method = HttpMethod.Post,
        RequestUri = new Uri("https://test1-5a3ac21.svc.us-west1-gcp-free.pinecone.io/vectors/upsert"),
        Headers =
        {
            { "accept", "application/json" },
            { "Api-Key", "76459ca8-165f-492f-a052-65c9c85071b8" },
        },
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    // Send the HTTP request and get the response
    using (var client = new HttpClient())
    {
        var response = await client.SendAsync(request);
        return response.StatusCode;
    }

    }      
      
}

