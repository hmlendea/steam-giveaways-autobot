using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Microsoft.Extensions.Configuration;

using SteamGiveawaysAutoBot.Bots;
using SteamGiveawaysAutoBot.Entities;
using SteamGiveawaysAutoBot.Utils;

namespace SteamGiveawaysAutoBot
{
    public sealed class Program
    {
        static IBot bot;
        
        static void Main(string[] args)
        {
            IConfiguration config = LoadConfiguration();

            AccountDetails account = new AccountDetails
            {
                Username = config["steamUsername"],
                Password = config["steamPassword"],
                SteamGiftsSessionId = config["steamGiftsSessionId"]
            };
            
            bot = new SteamGiftsBot(account);
            bot.Start();

            while (true)
            {
                string cmd = Console.ReadLine().ToLower();

                if (cmd == "exit")
                {
                    bot.Stop();
                    Environment.Exit((int)ExitCodes.Success);
                }
            }
        }
        
        static void Exit(ExitCodes exitCode)
        {
            int exitCodeValue = (int)exitCode;

            Log.Error($"Application exited with exit code {exitCodeValue} ({exitCode})");
            Environment.Exit(exitCodeValue);
        }

        static IConfiguration LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();
        }
    }
}
