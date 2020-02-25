using System;
using System.Net;

namespace SteamGiveawaysAutoBot.Extenions
{
    public class WebClientEx : WebClient
    {
        public CookieContainer CookieContainer { get; set; }

        public string UserAgent
        {
            get
            {
                return Headers[HttpRequestHeader.UserAgent];
            }
            set
            {
                Headers[HttpRequestHeader.UserAgent] = value;
            }
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = CookieContainer;
            }

            return request;
        }
    }
}