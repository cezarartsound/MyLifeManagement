using MyLifeManagement.MyLife.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLifeManagement.MyLife.Models
{
    public class OperationEntity : IComparable<OperationEntity>
    {
        public int ID { get; private set; }

        private string description;
        private string place;
        private string matchRule;
        private OperationType type;



        public event EventHandler Changed;
        private bool hasChanged = false;
        public bool IsChanged { get { return hasChanged; } private set { hasChanged = value; Changed?.Invoke(this, null); } }




        public string Description { get { return description; } set { description = value; IsChanged = true; } }
        public string Place { get { return place; } set { place = value; IsChanged = true; } }
        public string MatchRule { get { return matchRule; } set { matchRule = value; IsChanged = true; } }
        public OperationType Type { get { return type; } set { type = value; IsChanged = true; } }

        public OperationEntity(int ID, string desc, string place, string match, OperationType type)
        {
            this.ID = ID;
            this.description = desc;
            this.place = place;
            this.matchRule = match;
            this.type = type;
        }
        
        public override string ToString()
        {
            return string.Format("{0}, {1} ({2})", Description,Place,Type);
        }
        
        public int CompareTo(OperationEntity other)
        {
            return this.Description.CompareTo(other.Description);
        }
    }
}
