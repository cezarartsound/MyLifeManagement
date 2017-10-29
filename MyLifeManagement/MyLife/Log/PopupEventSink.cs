using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Events;
using MyLifeManagement.MyLife.Views;
using System.Windows;

namespace MyLifeManagement.MyLife.Logging
{
    public class PopupEventSink : ILogEventSink
    {
        private MainWindow mainWindow;

        public PopupEventSink(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        public void Emit(LogEvent logEvent)
        {
            MessageBox.Show(
                logEvent.MessageTemplate.Text,
                logEvent.Level.ToString(),
                MessageBoxButton.OK,
                logEvent.Level == LogEventLevel.Error?MessageBoxImage.Error:MessageBoxImage.Warning);
        }
    }
}
