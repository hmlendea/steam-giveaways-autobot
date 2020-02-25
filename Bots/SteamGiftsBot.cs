using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Timers;

using HtmlAgilityPack;

using SteamGiveawaysAutoBot.Entities;
using SteamGiveawaysAutoBot.Extenions;
using SteamGiveawaysAutoBot.Utils;

namespace SteamGiveawaysAutoBot.Bots
{
    public sealed class SteamGiftsBot : Bot
    {
        protected override int MinimumPoints => 20;

        protected override int MaximumPoints => 400;

        Giveaway lastProcessedGiveaway;

        public override string GiveawaysProviderName => "SteamGifts";

        public override string HomePageUrl => "https://www.steamgifts.com";

        public SteamGiftsBot(AccountDetails profile)
            : base(profile)
        {
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
        }

        protected override void RetrieveInitialInformation()
        {
            GoToUrl(HomePageUrl);

            string loginButtonXpath = @"//a[@class='nav__sits']";
            string avatarXpath = @"//a[@class='nav__avatar-outer-wrap']";

            if (DoesElementExist(loginButtonXpath))
            {
                Log.Error($"User is NOT logged in");
                Stop();
                
                return;
            }
            else
            {
                Log.Info($"User is logged in");
            }

            string profileRelLink = GetAttribute(avatarXpath, "href");
            string profileUrl = $"{HomePageUrl}{profileRelLink}";

            Log.Info($"Profile URL: {profileUrl}");
        }

        protected override void SendRequest()
        {
            GoToUrl(HomePageUrl);

            string loginButtonXpath = @"//a[@class='nav__sits']";
            string pointsXpath = @"//span[@class='nav__points']";
            string suspensionsButtonXpath = @"//div[contains(@class,'nav__button-container')]/a[@href='/suspensions']";

            if (DoesElementExist(loginButtonXpath) || !DoesElementExist(pointsXpath))
            {
                IsLoggedIn = false;
                throw new Exception("This account is not logged in");
                //return;
            }

            if (DoesElementExist(suspensionsButtonXpath))
            {
                throw new Exception("This account is suspended");
            }

            CurrentPoints = GetCurrentPoints();
            CurrentRewards = GetCurrentRewards();

            if (CurrentPoints < MinimumPoints)
            {
                TimerInterval = RechargeInterval;

                return;
            }
            else
            {
                TimerInterval = DefaultInterval;
            }

            Giveaway giveaway = FindGiveaway();
            
            ProcessGiveaway(giveaway);
        }

        int GetCurrentPoints()
        {
            string pointsXpath = @"//span[@class='nav__points']";
            string pointsString = GetText(pointsXpath);

            return int.Parse(pointsString);
        }

        int GetCurrentRewards()
        {
            string pointsXpath = @"/html/body/header/nav/div[2]/div[2]/a";
            string pointsString = GetText(pointsXpath);

            if (string.IsNullOrWhiteSpace(pointsString))
            {
                return 0;
            }

            return int.Parse(pointsString);
        }

        Giveaway FindGiveaway()
        {
            //Log.Info($"Finding a giveaway to join");
            GoToUrl(HomePageUrl);

            for (int i = 1; i <= 50; i++)
            {
                string giveawayXpath = $"//div[@class='widget-container']/div[2]/div[3]/div[{i}]/div/div/h2/a[@class='giveaway__heading__name']";

                string giveawayLink = GetHyperlink(giveawayXpath);

                Giveaway giveaway = new Giveaway(giveawayLink);

                if (lastProcessedGiveaway != giveaway)
                {
                    return giveaway;
                }
            }

            return null;
        }

        void ProcessGiveaway(Giveaway giveaway)
        {
            GoToUrl(giveaway.Url);

            lastProcessedGiveaway = giveaway;

            string gameTitleXpath = @"//div[@class='featured__heading__medium']";
            string priceXpath = @"//div[@class='featured__heading__small']";
            string remaininTimeXpath = @"//div[@class='featured__summary']/div[2]/div[1]/span";
            string errorXpath = @"//div[contains(@class,'sidebar__error') or contains(@class,'sidebar__entry-delete')]";
            string tokenXpath = @"//input[@name='xsrf_token']";

            string gameTitle = GetText(gameTitleXpath);
            string priceText = GetText(priceXpath);
            string endTimeUnixTimestamp = GetAttribute(remaininTimeXpath, "data-timestamp");
            string priceSubstring = priceText.Substring(1).Substring(0, priceText.Length - 3);

            int price = int.Parse(priceSubstring);

            DateTime endTime = DateTimeUtils.FromUnixTime(endTimeUnixTimestamp);

            //Log.Info($"Giveaway info retrieved for '{giveaway.GiveawayCode}': \"{gameTitle}\" ({price}P), ends at {endTime}");

            if (endTime > DateTime.Now)
            {
                TimerInterval = endTime - DateTime.Now;
            }

            if (DoesElementExist(errorXpath))
            {
                string errorText = GetText(errorXpath).Trim();;

                if (errorText.Contains("Remove Entry"))
                {
                    errorText = "Already entered";
                }

                Log.Warn($"Cannot enter '{giveaway.GiveawayCode}': \"{errorText}\"");
            }
            else
            {
                string token = GetValue(tokenXpath);
                
                EnterGiveaway(giveaway, token);
            }
        }

        void EnterGiveaway(Giveaway giveaway, string token)
        {
            //Log.Info($"Entering giveaway '{giveaway.GiveawayCode}'");

            string url = $"{HomePageUrl}/ajax.php";

            NameValueCollection data = new NameValueCollection
            {
                { "xsrf_token", token },
                { "do", "entry_insert" },
                { "code", giveaway.GiveawayCode }
            };

            WebClientEx client = new WebClientEx();
            client.UserAgent = UserAgent;
            client.CookieContainer = Cookies;

            client.UploadValues(url, data);
            client.Dispose();
        }
    }
}
