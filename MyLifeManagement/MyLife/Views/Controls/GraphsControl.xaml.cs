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
using Serilog;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;

namespace MyLifeManagement.MyLife.Views.Controls
{
    /// <summary>
    /// Interaction logic for TotalsTypeControl.xaml
    /// </summary>
    public partial class GraphsControl : UserControl, ITimeLoadableControl, ITypeFiltered
    {
        public List<OperationType> FilterTypes { get; } = new List<OperationType>();
        public ObservableCollection<Operation> OperationsList { get; } = new ObservableCollection<Operation>();

        private DateTime from;
        private DateTime to;

        public GraphsControl()
        {
            InitializeComponent();

            DataContext = this;

            Loaded += new RoutedEventHandler(Control_Loaded);
        }

        public void Load(DateTime from, DateTime to)
        {
            OperationsList.Clear();

            this.from = from;
            this.to = to;

            foreach (var o in DAL.GetOperations(from, to))
                OperationsList.Add(o);

            if (OperationsList.Count == 0)
                Log.Warning("No operation for that period!");

            Control_Loaded(this, null);
        }

        public void Filter(OperationType[] filter)
        {
            FilterTypes.Clear();
            FilterTypes.AddRange(filter);

            Control_Loaded(this, null);
        }

        private bool IsFiltered(Operation op)
        {
            if (op.ForcedType != null)
                return !FilterTypes.Contains(op.ForcedType);
            else if (op.ForcedEntity != null)
                return !FilterTypes.Contains(op.ForcedEntity.Type);
            else if (op.AutoEntity != null)
                return !FilterTypes.Contains(op.AutoEntity.Type);
            else
                return !FilterTypes.Contains(OperationType.Empty);
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            var operations = new List<Operation>();
            
            Operation curr = null;

            foreach(var op in OperationsList)
            {
                if (IsFiltered(op))
                    continue;

                if (curr == null)
                {
                    curr = new Operation(0,
                        op.Date.AddDays(-1),
                        string.Format("{0} ({1})", op.Date.AddDays(-1).ToShortDateString(), op.CurrentBalance.Value - op.OperationMove.Value),
                        new OperationValue(0),
                        new OperationValue(op.CurrentBalance.Value - op.OperationMove.Value),
                        null);

                    operations.Add(curr);
                }

                if ((curr == null) || (curr.Date != op.Date))
                {
                    curr = new Operation(op.ID, 
                        op.Date,
                        string.Format("{0}\r\n  {1}", op.Date.ToShortDateString(), op.ToString()),
                        (OperationValue)op.OperationMove.Clone(),
                        (OperationValue)op.CurrentBalance.Clone(),
                        null);

                    operations.Add(curr);
                }
                else
                {
                    curr.Description += "\r\n  " + op.ToString();
                    curr.OperationMove.Value += op.OperationMove.Value;
                    curr.CurrentBalance.Value = op.CurrentBalance.Value;
                }
            }

            DateTime[] dates = operations.Select<Operation, DateTime>((o) => o.Date).ToArray();
            

            var currBalanceDataSource = new EnumerableDataSource<Operation>(operations);
            currBalanceDataSource.SetXMapping(x => dateAxis.ConvertToDouble(x.Date));
            currBalanceDataSource.SetYMapping(y => y.CurrentBalance.Value);
            currBalanceDataSource.AddMapping(CircleElementPointMarker.ToolTipTextProperty, y => y.Description);

            plotter.Children.RemoveAll(typeof(LineGraph));
            plotter.Children.RemoveAll(typeof(ElementMarkerPointsGraph));

            plotter.AddLineGraph(currBalanceDataSource,
              new Pen(Brushes.Blue, 2),
              new CircleElementPointMarker { Size = 10.0, Fill = Brushes.Blue, Brush = Brushes.Blue },
              new PenDescription("Current balance"));
            
            plotter.Viewport.FitToView();
        }
    }
}
