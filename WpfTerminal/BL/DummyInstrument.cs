using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfTerminal.BL
{
    public enum Axis
    {
        X,
        Y
    }

    public enum StepSize
    {
        Fine,
        Coarse
    }
    public class DummyInstrument
    {
        //public Dictionary<Axis, int> Axis = new Dictionary<BL.Axis, int>();

        private Dictionary<Axis,int> _axis;

        public Dictionary<Axis,int> Axis
        {
            get { return _axis; }
            set { _axis = value; }
        }

        public List<string> GetM (string B)
        { 
            var l = new List<string>();
            l.Add("M1");
            l.Add("M very very very very long");
            l.Add("M3");

            return l;
        }

        public List<string> GetB()
        {
            var l = new List<string>();
            l.Add("B1");
            l.Add("B very very very very long");
            l.Add("B3");

            return l;
        }

       public void Move(Dictionary<Axis, int> _newaxis, StepSize step, string M , string B)
        {
            MessageBox.Show(
                            "B= " + B + Environment.NewLine +
                            "M= " + M + Environment.NewLine +
                            "StepSizr= " + step + Environment.NewLine+
                            "Axis.x= " +_newaxis[BL.Axis.X]+Environment.NewLine+
                            "Axis.y= " + _newaxis[BL.Axis.Y]+Environment.NewLine
                            );
        }

    }
}
