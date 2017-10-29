using MyLifeManagement.MyLife.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MyLifeManagement.MyLife.Models;
using System.Collections.ObjectModel;

namespace MyLifeManagement.MyLife.Views.Controls
{
    /// <summary>
    /// Interaction logic for TotalsTypeControl.xaml
    /// </summary>
    public partial class TotalsTypeControl : UserControl, ITimeLoadableControl, ITypeFiltered
    {
        public List<OperationType> FilterTypes { get; } = new List<OperationType>();
        public CollectionViewSource TotalsView { get; } = new CollectionViewSource();
        public ObservableCollection<OperationTotal> TotalsList { get; } = new ObservableCollection<OperationTotal>();

        public TotalsTypeControl()
        {
            InitializeComponent();

            TotalsView.Source = TotalsList;

            TotalsView.View.Filter = (o) =>
            {
                return FilterTypes.Count((x) => (o as OperationTotal).Description == x.Description) > 0;
            };

            totalTypeGrid.ItemsSource = TotalsView.View;
        }

        public void Load(DateTime from, DateTime to)
        {
            TotalsList.Clear();

            foreach (var o in DAL.GetTotalsForTypes(from, to))
                TotalsList.Add(o);
        }
        
        public void Filter(OperationType[] filter)
        {
            FilterTypes.Clear();
            FilterTypes.AddRange(filter);

            TotalsView.View.Refresh();
        }
    }
}
