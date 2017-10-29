using MyLifeManagement.MyLife.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLifeManagement.MyLife.Models
{
    public class OperationType : IComparable<OperationType>, IEquatable<OperationType>
    {

        public static OperationType Empty { get; } = new OperationType(-1, "");


        public int ID { get; private set; }



        public event EventHandler Changed;
        private bool hasChanged = false;
        public bool IsChanged { get { return hasChanged; } private set { hasChanged = value; Changed?.Invoke(this, null); } }



        private string description;
        public string Description { get { return description; } set { description = value; IsChanged = true; } }

        public OperationType(int id, string desc)
        {
            this.ID = id;
            this.description = desc;
        }

        public override string ToString()
        {
            return Description;
        }

        public int CompareTo(OperationType other) // Order
        {
            return this.Description.CompareTo(other.Description);
        }

        public bool Equals(OperationType other)
        {
            return this.ID == other.ID;
        }
    }
}
