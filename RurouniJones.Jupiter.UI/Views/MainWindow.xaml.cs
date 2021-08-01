using System.Collections.Specialized;
using System.Windows;

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
    }
}
