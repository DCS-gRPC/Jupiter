using System.Collections.Generic;

namespace RurouniJones.Jupiter.Encyclopedia.Entries

{
    public class Vehicle
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public List<string> DcsCodes { get; set; }
        public List<string> Attributes { get; set; }
    }
}
