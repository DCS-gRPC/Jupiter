using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace RurouniJones.Jupiter.Encyclopedia
{
    public class Unit
    {
        public string Name { get; set; }
        public string Code { get; set; }
        [YamlMember(Alias = "mil_std_2525_d")]
        public ulong MilStd2525D { get; set; }
        public List<string> DcsCodes { get; set; }
    }
}
