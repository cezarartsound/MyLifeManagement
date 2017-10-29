using MyLifeManagement.MyLife.Models;
using MyLifeManagement.MyLife.Views.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLifeManagement.MyLife.Views.Controls
{
    public interface IUpdatableControl
    {
        event EventHandler Changed;
        bool IsChanged { get; }
        bool SaveChanges();
    }
    public interface ILoadableControl
    {
        void Load();
    }

    public interface ITimeLoadableControl
    {
        void Load(DateTime from, DateTime to);
    }

    public interface ITypeFiltered
    {
        void Filter(OperationType[] filter);
    }
}
