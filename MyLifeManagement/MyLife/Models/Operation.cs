using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyLifeManagement.MyLife.Models
{
    public class Operation 
    {

        public int ID { get; set; } = 0;
        public DateTime Date {get; set; }
        public string Description { get; set; }
        public OperationValue OperationMove { get; set; } = new OperationValue(0);
        public OperationValue CurrentBalance { get; set; } = new OperationValue(0);
        public OperationEntity AutoEntity { get; set; } = null;


        public event EventHandler Changed;
        private bool hasChanged = false;
        public bool HasChanged { get { return hasChanged; } private set { hasChanged = value; Changed?.Invoke(this, null); } }


        private string notes = "";
        private bool exception = false;
        private OperationEntity forcedEntity = null;
        private OperationType forcedType = null;

        public string Notes
        {
            get { return notes; }
            set
            {
                notes = value;
                HasChanged = true;
            }
        }

        public bool Exception
        {
            get { return exception; }
            set
            {
                exception = value;
                HasChanged = true;
            }
        }
        
        public OperationEntity ForcedEntity
        {
            get { return forcedEntity; }
            set
            {
                forcedEntity = value;
                HasChanged = true;
            }
        }

        public OperationType ForcedType
        {
            get { return forcedType; }
            set
            {
                forcedType = value;
                HasChanged = true;
            }
        }



        public Operation()
        {
        }
        
        public Operation(int id, DateTime date, string desc, OperationValue operation, OperationValue currentBalance, OperationEntity autoEntity, 
            string notes = "", bool exception = false, OperationEntity forcedEntity = null, OperationType forcedType = null)
            :this()
        {
            this.ID = id;
            this.Date = date;
            this.Description = desc;
            this.OperationMove = operation;
            this.CurrentBalance = currentBalance;
            this.AutoEntity = autoEntity;

            this.notes = notes;
            this.exception = exception;
            this.forcedEntity = forcedEntity;
            this.forcedType = forcedType;
        }


        public override string ToString()
        {
            string desc = "";

            if (ForcedEntity != null)
                desc += ForcedEntity.Description;
            else if (AutoEntity != null)
                desc += AutoEntity.Description;
            else
                desc = Description;

            if (ForcedType != null)
                desc += string.Format(" ({0})", ForcedType.Description);
            else if (ForcedEntity != null)
                desc += string.Format(" ({0})", ForcedEntity.Type.Description);
            else if (AutoEntity != null)
                desc += string.Format(" ({0})", AutoEntity.Type.Description);
            
            return string.Format("{0} ({1}) - {2}", OperationMove.ToString(), CurrentBalance.ToString(), desc);
        }
    }
}
