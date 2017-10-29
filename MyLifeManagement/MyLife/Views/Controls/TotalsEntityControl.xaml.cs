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

namespace MyLifeManagement.MyLife.Views.Controls
{
    /// <summary>
    /// Interaction logic for TotalsEntityControl.xaml
    /// </summary>
    public partial class TotalsEntityControl : UserControl, ITimeLoadableControl
    {
        public TotalsEntityControl()
        {
            InitializeComponent();
        }
        
        public void Load(DateTime from, DateTime to)
        {
            totalEntityGrid.ItemsSource = DAL.GetTotalsForEntities(from, to);
        }
        
    }
}
