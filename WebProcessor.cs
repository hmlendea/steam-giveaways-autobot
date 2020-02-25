using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using HtmlAgilityPack;

namespace SteamGiveawaysAutoBot
{
    public class WebProcessor
    {
        public string UserAgent { get; set; }

        public CookieContainer Cookies { get; }
        
        HtmlDocument currentPage;

        public WebProcessor()
        {
            UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
            Cookies = new CookieContainer();
        }
        
        public void GoToUrl(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = UserAgent;
            request.Method = "GET";
            request.UserAgent = UserAgent;
            request.CookieContainer = Cookies;
            request.Headers.Add("Pragma", "no-cache");
            request.Timeout = 40000;
            request.AllowAutoRedirect = true;
            
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string html;

            using (Stream stream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                html = reader.ReadToEnd();
                reader.Close();
            }

            string fileName = url
                .Replace("https://", "")
                .Replace("http://", "")
                .Replace("/", "-");
            File.WriteAllText("page.html", html);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            currentPage = doc;
        }

        public string GetText(string xpath)
        {
            HtmlNode element = GetElement(xpath);
            string text = element.InnerText;

            if (string.IsNullOrEmpty(text) && element.Attributes.Any(x => x.Name == "value"))
            {
                text = element.Attributes["value"].Value;
            }

            return text;
        }

        public string GetHyperlink(string xpath) => GetAttribute(xpath, "href");
        public string GetSource(string xpath) => GetAttribute(xpath, "src");
        public string GetValue(string xpath) => GetAttribute(xpath, "value");

        public string GetAttribute(string xpath, string attribute)
        {
            HtmlNode element = GetElement(xpath);

            if (element is null)
            {
                Console.WriteLine("is null");
            }
            
            Console.WriteLine(xpath);
            foreach (var a in element.Attributes)
            {
                Console.WriteLine(a.Name + " " + a.Value);
            }

            string attributeValue = element.Attributes[attribute].Value;

            return attributeValue;
        }

        public bool DoesElementExist(string xpath)
        {
            HtmlNode element = GetElement(xpath);

            if (element is null)
            {
                return false;
            }

            return true;
        }

        HtmlNode GetElement(string xpath)
        {
            HtmlNode element = currentPage.DocumentNode.SelectSingleNode(xpath);
            return element;
        }

        IEnumerable<HtmlNode> GetElements(string xpath)
        {
            HtmlNodeCollection elements = currentPage.DocumentNode.SelectNodes(xpath);

            return elements.Select(x => x);
        }
    }
}
