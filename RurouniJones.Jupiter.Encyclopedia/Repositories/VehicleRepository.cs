using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RurouniJones.Jupiter.Encyclopedia.Repositories
{
    public class VehicleRepository
    {
        private static readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        private static readonly HashSet<Entries.Vehicle> Entries = Deserializer.Deserialize<HashSet<Entries.Vehicle>>(
            File.ReadAllText("Data/Encyclopedia/Vehicles.yaml"));

        #region Get Codes

        public static List<string> GetCodesByIdentifier(string identifier)
        {
            return Entries.FirstOrDefault(x => x.Name.ToLower() == identifier ||
                                               x.Code.ToLower() == identifier ||
                                               x.DcsCodes.Contains(identifier))?.DcsCodes;
        }

        public static List<string> GetCodesByAttributes(ISet<string> attributes)
        {
            return Entries.Where(e =>!attributes.Except(e.Attributes).Any()).SelectMany(x => x.DcsCodes).ToList();
        }

        #endregion

        public static string GetNameByDcsCode(string code)
        {
            var aircraft = Entries.FirstOrDefault(x => x.DcsCodes.Contains(code) || x.Code.Equals(code));
            return aircraft == null ? code : aircraft.Name;
        }

        public static string GetCodeByDcsCode(string code)
        {
            var aircraft = Entries.FirstOrDefault(x => x.DcsCodes.Contains(code));
            return aircraft == null ? code : aircraft.Code;
        }

        public static List<string> GetAttributesByDcsCode(string code)
        {
            var aircraft = Entries.FirstOrDefault(x => x.DcsCodes.Contains(code) || x.Code.Equals(code));
            return aircraft == null ? new List<string>() : aircraft.Attributes;
        }
    }
}
