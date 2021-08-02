using System.Collections.ObjectModel;

namespace RurouniJones.Jupiter.Core.Models
{
    public class Coalition : ModelBase
    {
        private uint _id;
        public uint Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private ObservableCollection<Group> _groups;
        public ObservableCollection<Group> Groups
        {
            get => _groups;
            set => SetProperty(ref _groups, value);
        }

        public static ObservableCollection<Coalition> DefaultCoalitions()
        {
            return new ObservableCollection<Coalition>()
            {
                new Coalition {Id = 0, Name = "Neutral", Groups = new ObservableCollection<Group>()},
                new Coalition {Id = 1, Name = "Redfor", Groups = new ObservableCollection<Group>()},
                new Coalition {Id = 2, Name = "Bluefor", Groups = new ObservableCollection<Group>()},
            };
        }
    }
}
