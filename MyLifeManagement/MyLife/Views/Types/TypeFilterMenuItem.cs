using MyLifeManagement.MyLife.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MyLifeManagement.MyLife.Views.Types
{
    public class TypeFilterMenuItem : MenuItem
    {
        public OperationType Type { get; protected set; }

        public TypeFilterMenuItem(OperationType type)
        {
            this.Type = type;
            this.Header = type.Description;
        }
    }
}
