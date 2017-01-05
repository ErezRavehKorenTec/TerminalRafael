using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfTerminal.Enums;

namespace WpfTerminal.BL
{
    public abstract class BaseConnections
    {
        #region CONSTS
        public const string FIRST_LINE_DISPLAY = "B: ";
        public const string SECOND_LINE_DISPLAY = "M: ";
        public const string THIRD_LINE_DISPLAY = "Step: ";
        #endregion

        public DummyInstrument test;
        public static readonly byte[] MOVE_CURSER_UP = new byte[] { 0x1B, 0x5B, 0x41 };
        public static readonly byte[] MOVE_CURSER_DOWN = new byte[] { 0x1B, 0x5B, 0x42 };
        public event EventHandler TerminalClickRecive;
        public int _bIndex = 0;
        public int _mIndex = 0;

        #region Properties

        private int _curserLine = 4;
        public int CurserLine
        {
            get { return _curserLine; }
            set { _curserLine = value; }
        }

        private int _LastCurserLocation = 4;

        public int LastCurserLocation
        {
            get { return _LastCurserLocation; }
            set { _LastCurserLocation = value; }
        }

        public List<string> MSelection { get; set; }
        public List<string> BSelection { get; set; }

        private StepSize _stepSelection = (StepSize)0;

        public StepSize SelectedStep
        {
            get { return _stepSelection; }
            set { _stepSelection = value; }
        }
        public bool ConnectionSucceded { get; set; }
        public int TerminalStepSize { get; set; }
        public Dictionary<string, string> TerminalParameters { get; set; }
        public bool IsConfirmed { get; set; }
        #endregion
        public virtual void WriteToTerminalScreen(string obj) { }
        public virtual void ReciveData() { }
        public abstract bool CloseConnection();
        public BaseConnections()
        {
            TerminalParameters = Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalPreferences);
            test = new DummyInstrument();
            BSelection = test.GetB();
            MSelection = test.GetM(BSelection.First());
            test.Axis = new Dictionary<Axis, int>() { { Axis.X, 0 }, { Axis.Y, 0 } };
            
        }

    }
}
