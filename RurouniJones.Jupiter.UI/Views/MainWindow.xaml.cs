using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RurouniJones.Jupiter.Core.Models;
using RurouniJones.Jupiter.Core.ViewModels;

namespace RurouniJones.Jupiter.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ((INotifyCollectionChanged)EventListView.ItemsSource).CollectionChanged +=
                (s, e) =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        EventListView.ScrollIntoView(EventListView.Items[^1]);
                    }
                };
        }

        private void TreeViewItem_OnSelected(object sender, RoutedEventArgs e)
        {
            ((MainViewModel) DataContext).MapLocation = e.OriginalSource switch
            {
                TreeViewItem item when item.DataContext is Unit unit => unit.Location,
                TreeViewItem item when item.DataContext is Group group => group.Units.First().Location,
                _ => ((MainViewModel) DataContext).MapLocation
            };
        }
    }
}
