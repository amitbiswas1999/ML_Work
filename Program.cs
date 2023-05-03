using System.ComponentModel;
using System.Net;
using System.Collections.Generic;
using HtmlAgilityPack;
using System;
using System.IO;
using System.Text.RegularExpressions;


internal class Program

{
    private static void Main(string[] args)
    {
        string url="https://www.cloudcraftz.com/";
        int level=3;

        List<string> web_url = new List<string>();
        web_url.Add(url);

        var doc= GetDocument(url);

        var Links=ExtractLinks(doc);
    
        var all_links=FilteredLink(web_url,level);

    //    Console.WriteLine(String.Join(Environment.NewLine, all_links));

        Linktotext(all_links);
       
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

        return outputtext ;
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

    static void Linktotext(List<String>links)
    {
        foreach(var items in links)
        {
            var doc= GetDocument(items);
            var str=Gettext(doc);
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
      
    }

}