using MyLifeManagement.MyLife.Database;
using MyLifeManagement.MyLife.Models;
using MyLifeManagement.MyLife.Views.Types;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace MyLifeManagement.MyLife.Views.Controls
{
    /// <summary>
    /// Interaction logic for OperationsView.xaml
    /// </summary>
    public partial class OperationsControl : UserControl, IUpdatableControl, ITimeLoadableControl, ITypeFiltered
    {
        public event EventHandler Changed;
        public bool IsChanged { get; private set; } = false;

        public List<OperationType> FilterTypes { get; } = new List<OperationType>();
        public CollectionViewSource OperationsView { get; } = new CollectionViewSource();
        public ObservableCollection<Operation> OperationsList { get; } = new ObservableCollection<Operation>();

        private DateTime from;
        private DateTime to;

        public OperationsControl()
        {
            InitializeComponent();

            OperationsView.Source = OperationsList;

            OperationsView.View.Filter = (o) =>
            {
                var op = o as Operation;

                if (op.ForcedType != null)
                    return FilterTypes.Contains(op.ForcedType);
                else if (op.ForcedEntity != null)
                    return FilterTypes.Contains(op.ForcedEntity.Type);
                else if (op.AutoEntity != null)
                    return FilterTypes.Contains(op.AutoEntity.Type);
                else
                    return FilterTypes.Contains(OperationType.Empty);
            };

            operationsDataGrid.ItemsSource = OperationsView.View;
        }
        
        public void Load(DateTime from, DateTime to)
        {
            OperationsList.Clear();

            this.from = from;
            this.to = to;

            foreach (var o in DAL.GetOperations(from, to))
            {
                o.Changed += (x, y) => Changed?.Invoke(this, null);
                o.Changed += (x, y) => IsChanged = true;
                OperationsList.Add(o);
            }
            
            if (OperationsList.Count == 0)
                Log.Warning("No operation for that period!");
        }

        public bool SaveChanges()
        {
            int changed = 0;

            foreach (Operation e in OperationsList)
                if (e.HasChanged)
                    changed++;

            if (changed == 0)
            {
                MessageBox.Show("No changes to update.", "Update", MessageBoxButton.OK);
                return true;
            }

            switch (MessageBox.Show("Do you want to update " + changed + " operations in Database?", "Update", MessageBoxButton.YesNo))
            {
                case MessageBoxResult.No:
                    return false;
            }
            
            foreach (Operation o in OperationsList)
                if (o.HasChanged)
                    DAL.UpdateOperation(o);

            MessageBox.Show("Updated " + changed + " operations in Database", "Update", MessageBoxButton.OK);

            IsChanged = false;
            return true;
        }

        private void rerunAutoEntity_Click(object sender, RoutedEventArgs e)
        {
            if(operationsDataGrid.SelectedItems.Count == 0)
            {
                MessageBox.Show("No operation selected. Please select at least one.", "Re-run auto entity", MessageBoxButton.OK);
                return;
            }

            if (MessageBox.Show("Do you want to find the entity for "+ operationsDataGrid.SelectedItems.Count + " operation(s)?", "Re-run auto entity", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                foreach(Operation o in operationsDataGrid.SelectedItems)
                    DAL.RunAutoentity(o);

                this.Load(from, to);
            }
        }
        
        public void Filter(OperationType[] filter)
        {
            FilterTypes.Clear();
            FilterTypes.AddRange(filter);

            OperationsView.View.Refresh();
        }
    }
}
