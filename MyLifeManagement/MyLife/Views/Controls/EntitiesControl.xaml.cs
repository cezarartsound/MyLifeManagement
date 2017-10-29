using MyLifeManagement.MyLife.Database;
using MyLifeManagement.MyLife.Models;
using Serilog;
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
    public partial class EntitiesControl : UserControl, IUpdatableControl, ILoadableControl
    {
        public event EventHandler Changed;
        public bool IsChanged { get; private set; } = false;

        private List<OperationEntity> entities = new List<OperationEntity>();

        public EntitiesControl()
        {
            InitializeComponent();
        }

        public void Load()
        {
            entities.Clear();

            var ts = DAL.GetOperationEntities();

            foreach (var t in ts)
            {
                t.Changed += (x, y) => Changed?.Invoke(this, null);
                t.Changed += (x, y) => IsChanged = true;
                entities.Add(t);
            }

            entitiesGrid.ItemsSource = entities;
        }

        public bool SaveChanges()
        {
            int changed = 0;
            int inserted = 0;

            foreach (OperationEntity e in entitiesGrid.Items)
                if (e.ID == -1)
                    inserted++;
                else if (e.IsChanged)
                    changed++;

            if(changed == 0 && inserted == 0)
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

                foreach (OperationEntity e in entitiesGrid.Items)
                    if (e.ID != -1 && e.IsChanged)
                        DAL.UpdateEntity(e);

                MessageBox.Show("Updated " + changed + " entries in Database.", "Update", MessageBoxButton.OK);
            }

            if (inserted > 0)
            {
                switch (MessageBox.Show("Do you want to insert new " + inserted + " entries in Database?", "Update", MessageBoxButton.YesNo))
                {
                    case MessageBoxResult.No:
                        return false;
                }

                foreach (OperationEntity e in entitiesGrid.Items)
                    if (e.ID == -1)
                        DAL.InsertEntity(e);

                MessageBox.Show("Inserted " + inserted + " entries in Database.", "Update", MessageBoxButton.OK);
            }

            //System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            //Application.Current.Shutdown();

            IsChanged = false;
            return true;
        }
        
        private void addItem_Click(object sender, RoutedEventArgs e)
        {
            var newlist = new List<OperationEntity>();

            var t = new OperationEntity(-1, "<Description>", "", "<MatchRule>", null);

            t.Changed += (x, y) => Changed?.Invoke(this, null);
            t.Changed += (x, y) => IsChanged = true;

            Changed?.Invoke(this, null);
            IsChanged = true;

            newlist.Add(t);
            newlist.AddRange(entities);

            entities = newlist;
            entitiesGrid.ItemsSource = newlist;
        }
    }
}
