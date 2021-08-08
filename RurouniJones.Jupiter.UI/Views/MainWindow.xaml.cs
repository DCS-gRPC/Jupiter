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
            var viewModel = (MainViewModel)DataContext;
            var treeViewItem = (TreeViewItem) e.OriginalSource;


            switch (treeViewItem.DataContext)
            {
                case Unit unit:
                {
                    viewModel.SelectedUnit = unit;
                    viewModel.MapLocation = unit.Location;
                    break;
                }
                case Group group:
                    viewModel.MapLocation = @group.Units.First().Location;
                    break;
            }
        }
    }
}
