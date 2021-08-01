using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RurouniJones.Jupiter.Core.Models
{
    public class Group : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Location _name;
        public Location Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged(nameof(Location));
            }
        }

        private ObservableCollection<Unit> _units;
        public ObservableCollection<Unit> Units
        {
            get => _units;
            set
            {
                _units = value;
                RaisePropertyChanged(nameof(Location));
            }
        }
    }
}
