using System;
using System.Xml.Serialization;

namespace SteamGiveawaysAutoBot.Entities
{
    public sealed class CookieData
    {
        [XmlAttribute]
        public string Domain { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlText]
        public string Value { get; set; }

        public override string ToString()
        {
            return $"({Domain}){Name}={Value}";
        }
    }
}
