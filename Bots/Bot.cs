using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Timers;

using HtmlAgilityPack;

using SteamGiveawaysAutoBot.Entities;

namespace SteamGiveawaysAutoBot.Bots
{
    public abstract class Bot : WebProcessor, IBot
    {
        protected abstract int MinimumPoints { get; }

        protected abstract int MaximumPoints { get; }

        public int CurrentPoints { get; protected set; }

        public int CurrentRewards { get; protected set; }

        protected Random Random;
        protected AccountDetails account;

        Timer timer;
        public bool IsRunning { get; private set; }

        public bool IsLoggedIn { get; protected set; }

        public TimeSpan TimerInterval
        {
            get { return TimeSpan.FromMilliseconds(timer.Interval); }
            set { UpdateTimerInterval(value); }
        }

        public TimeSpan RechargeInterval => TimeSpan.FromMinutes(30);

        protected TimeSpan DefaultInterval => TimeSpan.FromSeconds(30);

        public abstract string GiveawaysProviderName { get; }

        public abstract string HomePageUrl { get; }

        public Bot(AccountDetails profile)
        {
            account = profile;
            Random = new Random();
            
            SetTimer();
        }

        public virtual void Start()
        {
            //Log.Info($"Starting {GetType().Name} bot for {account.Username}");
            IsRunning = true;

            AddSessionCookie();

            //Log.Info($"Gathering initial information");
            RetrieveInitialInformation();

            timer.Enabled = true;
        }

        public virtual void Stop()
        {
            Log.Info($"Stopping {GetType().Name} bot for {account.Username}");

            IsRunning = false;
            timer.Enabled = false;
        }

        protected abstract void RetrieveInitialInformation();

        protected abstract void SendRequest();

        void AddSessionCookie()
        {
            Cookie cookie = new Cookie();
            cookie.Name = "PHPSESSID";
            cookie.Value = "02913ef4b931d3dbb9650823cc1c6347";
            cookie.Domain = ".www.steamgifts.com";
            cookie.Path = "/";
            cookie.HttpOnly = false;
            cookie.Secure = false;
            cookie.Expires = DateTime.Now.AddDays(2);
            cookie.Expired = false;

            Cookies.Add(cookie);
        }

        void SetTimer()
        {
            timer = new Timer(10000);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = false;
        }

        void UpdateTimerInterval(TimeSpan interval)
        {
            int min = (int)(Math.Max(
                interval.TotalMilliseconds - 10000,
                interval.TotalMilliseconds - (int)(interval.TotalMilliseconds * 0.2)));

            int max = (int)(Math.Min(
                interval.TotalMilliseconds + 10000,
                interval.TotalMilliseconds + (int)(interval.TotalMilliseconds * 0.2)));

            timer.Interval = Random.Next(min, max);
        }

        void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            SendRequest();
        }
    }
}
