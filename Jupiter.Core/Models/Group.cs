using System.Collections.ObjectModel;

namespace RurouniJones.Jupiter.Core.Models
{
    public class Group : ModelBase
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

        private ObservableCollection<Unit> _units;
        public ObservableCollection<Unit> Units
        {
            get => _units;
            set => SetProperty(ref _units, value);
        }
    }
}
