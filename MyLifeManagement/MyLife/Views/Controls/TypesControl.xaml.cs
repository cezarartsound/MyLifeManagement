using MyLifeManagement.MyLife.Database;
using MyLifeManagement.MyLife.Models;
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

namespace MyLifeManagement.MyLife.Views.Controls
{
    /// <summary>
    /// Interaction logic for EntitiesControl.xaml
    /// </summary>
    public partial class TypesControl : UserControl, IUpdatableControl, ILoadableControl
    {
        public event EventHandler Changed;
        public bool IsChanged { get; private set; } = false;

        private List<OperationType> types = new List<OperationType>();

        public TypesControl()
        {
            InitializeComponent();
        }

        public void Load()
        {
            types.Clear();

            var ts = DAL.GetOperationTypes();

            foreach(var t in ts)
            {
                t.Changed += (x, y) => Changed?.Invoke(this, null);
                t.Changed += (x, y) => IsChanged = true;
                types.Add(t);
            }

            typesGrid.ItemsSource = types;
        }

        public bool SaveChanges()
        {
            int changed = 0;
            int inserted = 0;

            foreach (OperationType e in typesGrid.Items)
                if (e.ID == -1)
                    inserted++;
                else if (e.IsChanged)
                    changed++;

            if (changed == 0 && inserted == 0)
            {
                MessageBox.Show("No changes to update.", "Update", MessageBoxButton.OK);
                return true;
            }

            if (changed > 0)
            {
                switch (MessageBox.Show("Do you want to update " + changed + " changes in Database?", "Update", MessageBoxButton.YesNo))
                {
                    case MessageBoxResult.No:
                        return false;
                }

                foreach (OperationType e in typesGrid.Items)
                    if (e.ID != -1 && e.IsChanged)
                        DAL.UpdateType(e);

                MessageBox.Show("Updated " + changed + " entries in Database.", "Update", MessageBoxButton.OK);
            }

            if (inserted > 0)
            {
                switch (MessageBox.Show("Do you want to insert new " + inserted + " entries in Database?", "Update", MessageBoxButton.YesNo))
                {
                    case MessageBoxResult.No:
                        return false;
                }

                foreach (OperationType e in typesGrid.Items)
                    if (e.ID == -1)
                        DAL.InsertType(e);

                MessageBox.Show("Inserted " + inserted + " entries in Database.", "Update", MessageBoxButton.OK);
            }

            //System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            //Application.Current.Shutdown();

            IsChanged = false;
            return true;
        }

        private void addItem_Click(object sender, RoutedEventArgs e)
        {
            var newlist = new List<OperationType>();

            var t = new OperationType(-1, "<Description>");

            t.Changed += (x, y) => Changed?.Invoke(this, null);
            t.Changed += (x, y) => IsChanged = true;

            Changed?.Invoke(this, null);
            IsChanged = true;

            newlist.Add(t);
            newlist.AddRange(types);

            types = newlist;
            typesGrid.ItemsSource = types;
        }
    }
}
