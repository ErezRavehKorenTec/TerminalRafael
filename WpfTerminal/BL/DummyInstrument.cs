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
    class DummyInstrument
    {
        List<string> GetM (string B)
        { 
            var l = new List<string>();
            l.Add("M1");
            l.Add("M very very very very long");
            l.Add("M3");

            return l;
        }

        List<string> GetB()
        {
            var l = new List<string>();
            l.Add("B1");
            l.Add("B very very very very long");
            l.Add("B3");

            return l;
        }

       void Move(Axis axis, StepSize step, string M , string B)
        {
            MessageBox.Show("Moved");
        }

    }
}
