using System;
using System.IO;
using System.Reflection;

namespace SteamGiveawaysAutoBot.Utils
{
    public sealed class ApplicationPaths
    {
        static string rootDirectory;

        /// <summary>
        /// The application directory.
        /// </summary>
        public static string ApplicationDirectory
        {
            get
            {
                if (rootDirectory == null)
                {
                    rootDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }

                return rootDirectory;
            }
        }

        public static string DataDirectory => Path.Combine(ApplicationDirectory, "Data");

        public static string ProfilesDirectory => Path.Combine(DataDirectory, "Profiles");

        public static string LogsDirectory => ApplicationDirectory;
    }
}