using MyLifeManagement.MyLife.Database;
using MyLifeManagement.MyLife.Logging;
using MyLifeManagement.MyLife.Models;
using MyLifeManagement.MyLife.Views.Controls;
using MyLifeManagement.MyLife.Views.Types;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace MyLifeManagement.MyLife.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Keeper.Load();
            entitiesControl.Load();
            typesControl.Load();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile("logs/log.txt", retainedFileCountLimit: 7, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose)
                .WriteTo.Sink(new PopupEventSink(this), restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
                .CreateLogger();

            Log.Information("Started"); 

            fromDatePicker.SelectedDate = DateTime.Now.AddMonths(-1);
            toDatePicker.SelectedDate = DateTime.Now;



            foreach (TabItem p in pagesTabControl.Items)
            {
                if (p.Content is IUpdatableControl)
                {
                    (p.Content as IUpdatableControl).Changed += (x, y) =>
                    {
                        writeDBButton.IsEnabled = true;
                    };
                }
            }



            #region Filter

            var selectAll = new MenuItem() { Header = "Select/Deselect all" };
            TypeFilter.Items.Add(selectAll);

            selectAll.IsChecked = true;
            selectAll.StaysOpenOnClick = true;

            selectAll.Click += (x, y) =>
            {
                bool sel = !selectAll.IsChecked;

                foreach (MenuItem t in TypeFilter.Items)
                {
                    t.IsChecked = sel;
                }

                ApplyTypeFilter();
            };

            List<OperationType> types = new List<OperationType>();

            types.AddRange(DAL.GetOperationTypes());
            types.Add(OperationType.Empty);
            
            foreach (var t in types)
            {
                var tf = new TypeFilterMenuItem(t);
                tf.IsChecked = true;
                tf.StaysOpenOnClick = true;
                tf.Click += (x, y) => tf.IsChecked = !tf.IsChecked;
                tf.Click += (x, y) => ApplyTypeFilter();

                TypeFilter.Items.Add(tf);
            }

            ApplyTypeFilter();

            #endregion
        }

        private void exitButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void aboutButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format("MyLifeManagement v{0}\r\nCreated by Fábio Cardoso. {1}",
                Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                (new DateTime(2000, 1, 1).AddDays(Assembly.GetExecutingAssembly().GetName().Version.Build)).ToString("MM/yyyy")),
                "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void importButtonClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".xls";
            dlg.Filter = "Excel file (*.xls)|*.xls|All Files (*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                try
                {

                    totalLabel.Content = "Loading Excel file, please wait...";

                    var app = new Microsoft.Office.Interop.Excel.Application();
                    var book = app.Workbooks.Open(dlg.FileName);
                    var sheet = book.ActiveSheet;

                    var operations = new List<Operation>();

                    for (int row = 8; sheet.Cells[row, 1].Value != null; row++)
                    {
                        operations.Add(new Operation(-1,
                            DateTime.ParseExact(sheet.Cells[row, 1].Value.ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture),
                            sheet.Cells[row, 3].Value.ToString(),
                            new OperationValue(double.Parse(sheet.Cells[row, 4].Value.ToString())),
                            new OperationValue(double.Parse(sheet.Cells[row, 5].Value.ToString())),
                            null
                        ));
                    }

                    if (MessageBox.Show(string.Format("Upload {0} operations to database?", operations.Count), "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        string output;
                        DAL.InsertOperations(operations.ToArray(), out output, true);

                        if (MessageBox.Show(output, "Apply the results?", MessageBoxButton.OKCancel, MessageBoxImage.None) == MessageBoxResult.OK)
                            DAL.InsertOperations(operations.ToArray(), out output);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            totalLabel.Content = "Ready";
        }

        private void readDB_Click(object sender, RoutedEventArgs e)
        {
            if (!fromDatePicker.SelectedDate.HasValue || !toDatePicker.SelectedDate.HasValue)
                return;


            if(writeDBButton.IsEnabled)
            {
                switch (MessageBox.Show("There are changes not saved. Do you want to discard them?", "Load", MessageBoxButton.YesNo))
                {
                    case MessageBoxResult.No:
                        return;
                }

                writeDBButton.IsEnabled = false;
            }


            DateTime from = fromDatePicker.SelectedDate.Value;
            DateTime to = toDatePicker.SelectedDate.Value;

            foreach (TabItem p in pagesTabControl.Items)
            {
                if (p.Content is ITimeLoadableControl)
                    (p.Content as ITimeLoadableControl).Load(from, to);
            }




            if (operationsControl.OperationsList.Count == 0)
            {
                totalLabel.Content = "No data";
            }
            else
            {
                OperationValue total = new OperationValue(0);
                OperationValue income = new OperationValue(0);
                OperationValue outcome = new OperationValue(0);

                foreach (var op in operationsControl.OperationsList)
                {
                    if (op.Exception) continue;

                    total.Value += op.OperationMove.Value;

                    if (op.OperationMove.Value > 0)
                        income.Value += op.OperationMove.Value;
                    else
                        outcome.Value += op.OperationMove.Value;
                }

                totalLabel.Content = string.Format("Start: {0}       End: {1}       Income: {2}       Outcome: {3}       Balance: {4}",
                    operationsControl.OperationsList[0].CurrentBalance, operationsControl.OperationsList[operationsControl.OperationsList.Count - 1].CurrentBalance, income, outcome, total);
            }
        }

        private void writeDB_Click(object sender, RoutedEventArgs e)
        {
            if (pagesTabControl.SelectedContent is IUpdatableControl)
            {
                if((pagesTabControl.SelectedContent as IUpdatableControl).SaveChanges())
                {
                    writeDBButton.IsEnabled = false;
                }
            }
        }

        private void pagesTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((e.AddedItems.Count > 0) && (e.AddedItems[0] is TabItem))
            {
                if ((e.AddedItems[0] as TabItem).Content is ITimeLoadableControl)
                {
                    timeLoadGrid.IsEnabled = true;
                }
                else
                {
                    timeLoadGrid.IsEnabled = false;
                }


                writeDBButton.IsEnabled = false;
                if ((e.AddedItems[0] as TabItem).Content is IUpdatableControl)
                {
                    if (((e.AddedItems[0] as TabItem).Content as IUpdatableControl).IsChanged)
                    {
                        writeDBButton.IsEnabled = true;
                    }
                }
            }

        }
        
        private void ApplyTypeFilter()
        {
            List<OperationType> filter = new List<OperationType>();

            foreach (var i in TypeFilter.Items)
            {
                if (i is TypeFilterMenuItem && (i as TypeFilterMenuItem).IsChecked)
                    filter.Add((i as TypeFilterMenuItem).Type);
            }

            foreach (TabItem p in pagesTabControl.Items)
            {
                if (p.Content is ITypeFiltered)
                    (p.Content as ITypeFiltered).Filter(filter.ToArray());
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            foreach (TabItem p in pagesTabControl.Items)
            {
                if ((p.Content is IUpdatableControl) && (p.Content as IUpdatableControl).IsChanged)
                {
                    switch (MessageBox.Show("There are changes not saved. Do you want to discard them?", "Exit", MessageBoxButton.YesNo))
                    {
                        case MessageBoxResult.No:
                            e.Cancel = true;
                            break;
                    }
                    break;
                }
            }

        }
    }
}
