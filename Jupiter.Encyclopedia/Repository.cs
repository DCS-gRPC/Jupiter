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

        private static readonly HashSet<Unit> Aircraft = Deserializer.Deserialize<HashSet<Unit>>(
            File.ReadAllText("Data/Encyclopedia/Aircraft.yaml"));
        private static readonly HashSet<Unit> Foot = Deserializer.Deserialize<HashSet<Unit>>(
            File.ReadAllText("Data/Encyclopedia/Foot.yaml"));
        private static readonly HashSet<Unit> Vehicles = Deserializer.Deserialize<HashSet<Unit>>(
            File.ReadAllText("Data/Encyclopedia/Vehicles.yaml"));
        private static readonly HashSet<Unit> Watercraft = Deserializer.Deserialize<HashSet<Unit>>(
            File.ReadAllText("Data/Encyclopedia/Watercraft.yaml"));

        private static readonly HashSet<Unit> Units = BuildUnitHashset();

        private static HashSet<Unit> BuildUnitHashset()
        {
            var set = new HashSet<Unit>(Aircraft);
            set.UnionWith(Foot);
            set.UnionWith(Vehicles);
            set.UnionWith(Watercraft);
            return set;
        }

        public static ulong GetMilStd2525DCodeByDcsCode(string code)
        {
            var milCode = Units.FirstOrDefault(x => x.DcsCodes.Contains(code) || x.Code.Equals(code))?.MilStd2525D;
            milCode ??= 10001000000000000000;
            Console.WriteLine($"Code {code} - Mil {milCode}");
            return (ulong) milCode;
        }
    }
}
