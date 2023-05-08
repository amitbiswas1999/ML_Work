using System.ComponentModel;
using System.Net;
using System.Collections.Generic;
using HtmlAgilityPack;
using System;
using System.IO;
using System.Text.RegularExpressions;
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


internal class Program

{
    
    private static async  Task Main(string[] args)
    {
        string url="https://www.cloudcraftz.com/";
        int level=3;

        List<string> web_url = new List<string>();
        web_url.Add(url);

        var doc= GetDocument(url);

        var Links=ExtractLinks(doc);
    
        var all_links=FilteredLink(web_url,level);

    //    Console.WriteLine(String.Join(Environment.NewLine, all_links));

        await Linktotext(all_links);
       
    }
    static List<String>FilteredLink (List<string>web_url,int level)
    {
        List<string>local_url=new List<string>();
        local_url.AddRange(web_url);
        while(level!=0)
        {
            foreach (var items in local_url.ToList())
            {
                var docs= GetDocument(items);
                var new_links=ExtractLinks(docs);
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
        // return doc; 
        return doc;
    }
    static string Gettext(HtmlDocument doc)
    {
        string str=doc.DocumentNode.InnerText.Trim();
        // string[] delimiters = new[] { "\r\n", "\r", "\n", "\n\n", "\n\n\n", "\n\n\n\n" };
        // string[] lines = str.Split(delimiters, StringSplitOptions.None);

        // string concatenatedString = string.Join("\n", lines);
        string cleanedText = Regex.Replace(str, @"\s+", " ");
        string outputtext = Regex.Replace(cleanedText, @"#\d+", "");
        string cleanedtext = Regex.Replace(outputtext, @"[^0-9a-zA-Z]+", " ").Trim();
        return cleanedtext ;
    }
    
    static List<string> ExtractLinks(HtmlDocument doc)
    {
        List<string> hrefTags = new List<string>();
        foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
        {
            HtmlAttribute att = link.Attributes["href"];
            hrefTags.Add(att.Value);
        }

        List<string> filteredList = hrefTags.Where(s => s.Contains("www.cloudcraftz")).ToList();

        return filteredList;
        
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
            Console.Write("embedding stores");


            string[] substrings = items.Split('/');
            string elem=substrings [substrings.Length-2];
            string txt=".txt";
            string file_name=elem +txt;

            using (StreamWriter writer = new StreamWriter(file_name))
            {
                writer.WriteLine(str);
            }

            Console.WriteLine("Text saved to file: " + elem);
        }

        string json = JsonConvert.SerializeObject(embeddingVector);

// Write the JSON string to a file
        File.WriteAllText(@"vector.json", json);
     
        Console.WriteLine(embeddingVector.ToString());   
    }

    static async Task<JToken>texttoemb (string text)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://api.openai.com/");
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer ");

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
            
            Console.WriteLine(embeddings);

            return embeddings;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return null;
        }

    }
        
      
}

