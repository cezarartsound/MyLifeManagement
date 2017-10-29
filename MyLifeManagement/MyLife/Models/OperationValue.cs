using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MyLifeManagement.MyLife.Models
{
    public class OperationValue : ICloneable
    {

        public string Type { get; set; } = "€";
        public double Value { get; set; }


        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public Brush Color
        {
            get
            {
                if (Value <= -500) return new SolidColorBrush(Colors.Red);
                else if (Value < -150) return new SolidColorBrush(Colors.Crimson);
                else if (Value < -70) return new SolidColorBrush(Colors.IndianRed);
                else if (Value < 0) return new SolidColorBrush(Colors.LightCoral);
                else if (Value < 100) return new SolidColorBrush(Colors.DarkSeaGreen);
                else if (Value < 300) return new SolidColorBrush(Colors.LightGreen);
                else return new SolidColorBrush(Colors.Green);
            }
            set { }
        }


        public OperationValue(double val)
        {
            this.Value = val;
        }
        public override string ToString()
        {
            //return string.Format("{0,10}",string.Format("{0:#0.00} {1}",Value, Type));
            return string.Format("{0}", string.Format("{0:#0.00} {1}", Value, Type));
        }
    }
}
