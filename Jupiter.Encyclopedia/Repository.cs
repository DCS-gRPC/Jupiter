using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RurouniJones.Jupiter.Encyclopedia
{
    public class Repository
    {
        private static readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        private static readonly HashSet<Unit> Air = Deserializer.Deserialize<HashSet<Unit>>(
            File.ReadAllText("Data/Encyclopedia/Air.yaml"));
        private static readonly HashSet<Unit> Land = Deserializer.Deserialize<HashSet<Unit>>(
            File.ReadAllText("Data/Encyclopedia/Land.yaml"));
        private static readonly HashSet<Unit> Sea = Deserializer.Deserialize<HashSet<Unit>>(
            File.ReadAllText("Data/Encyclopedia/Sea.yaml"));

        private static readonly HashSet<Unit> Units = BuildUnitHashset();

        private static HashSet<Unit> BuildUnitHashset()
        {
            var set = new HashSet<Unit>(Air);
            set.UnionWith(Land);
            set.UnionWith(Sea);
            return set;
        }

        public static ulong GetMilStd2525DCodeByDcsCode(string code)
        {
            var milCode = Units.FirstOrDefault(x => x.DcsCodes.Contains(code))?.MilStd2525D;
            ulong longCode;
            if(milCode != null) {
                longCode = ulong.Parse(milCode);
            }
            else {
                longCode = 10001000000000000000;
            }
            Console.WriteLine($"Code {code} - Mil {milCode}");
            return longCode;
        }
    }
}
