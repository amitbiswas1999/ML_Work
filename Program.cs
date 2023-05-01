using System.Collections.Generic;
using HtmlAgilityPack;
using System;
using System.IO;


internal class Program
{
    private static void Main(string[] args)
    {
        var doc= GetDocument(url:"https://www.cloudcraftz.com/");
        var str=Gettext(doc);
        
        string path="output1.txt";

        var Links=ExtractLinks(doc);
        // Console.WriteLine("Found {0} links", Links.Count);
        foreach(var item in Links)
        {
            
            string[] substrings = item.Split('/');
            string elem=substrings [substrings.Length-2];
            Console.WriteLine(elem);
        }

        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine(str);
        }

        Console.WriteLine("Text saved to file: " + path);

        // Console.Write(str);
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
        string[] delimiters = new[] { "\r\n", "\r", "\n", "\n\n", "\n\n\n", "\n\n\n\n" };
        string[] lines = str.Split(delimiters, StringSplitOptions.None);

        string concatenatedString = string.Join("\n", lines);
        return concatenatedString ;
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

        }
        
      
    }


}