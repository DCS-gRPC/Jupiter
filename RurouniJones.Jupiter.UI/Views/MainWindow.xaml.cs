using System.Collections.Specialized;
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

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UnitList.SelectedItems.Clear();

            if (!(sender is ListViewItem item)) return;
            item.IsSelected = true;
            UnitList.SelectedItem = item;
        }

        private void ListViewItem_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.IsSelected)
            {
                ((MainViewModel) DataContext).MapLocation = ((Unit) item.DataContext).Location;
            }
        }
    }
}
