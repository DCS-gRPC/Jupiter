using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RurouniJones.Jupiter.Encyclopedia.Repositories
{
    public class WatercraftRepository
    {
        private static readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        private static readonly HashSet<Entries.Watercraft> Entries = Deserializer.Deserialize<HashSet<Entries.Watercraft>>(
            File.ReadAllText("Data/Encyclopedia/Watercraft.yaml"));

        public static string GetNameByDcsCode(string code)
        {
            var aircraft = Entries.FirstOrDefault(x => x.DcsCodes.Contains(code));
            return aircraft == null ? code : aircraft.Name;
        }

        public static List<string> GetAttributesByDcsCode(string code)
        {
            var aircraft = Entries.FirstOrDefault(x => x.DcsCodes.Contains(code));
            return aircraft == null ? new List<string>() : aircraft.Attributes;
        }
    }
}
