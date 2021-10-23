namespace RurouniJones.Jupiter.Core.Models
{
    public class Symbology
    {
        public enum Context
        {
            Reality = 0,
            Exercise = 1,
            Simulation = 2
        }

        public enum StandardIdentity
        {
            Pending = 0,
            Unknown = 1,
            AssumedFriend = 2,
            Friend = 3,
            Neutral = 4,
            Suspect = 5,
            Hostile = 6
        }

        public enum SymbolSet
        {
            Air = 1
        }
    }
}
