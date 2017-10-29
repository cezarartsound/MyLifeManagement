using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLifeManagement.MyLife.Models
{
    public class OperationTotal
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string Description { get; set; }
        public OperationValue Total { get; set; }

        public OperationValue TotalDay
        {
            get
            {
                return new OperationValue(Total.Value / (To - From).TotalDays);
            }
        }

        public OperationValue TotalMonth
        {
            get
            {
                return new OperationValue(Total.Value / ((To - From).TotalDays / (365.25 / 12)));
            }
        }

        public OperationTotal(string desc, OperationValue operationValue, DateTime from, DateTime to)
        {
            this.Description = desc;
            this.Total = operationValue;
            this.From = from;
            this.To = to;
        }
    }
}
