﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WpfTerminal.Enums;

namespace WpfTerminal.BL
{
    public class ConnectionHandler
    {
        #region params


        public const string FIRST_LINE_DISPLAY = "B: ";
        public const string SECOND_LINE_DISPLAY = "M: ";
        public const string THIRD_LINE_DISPLAY = "Step: ";
        private static readonly byte[] MOVE_CURSER_UP = new byte[] { 0x1B, 0x5B, 0x41 };
        private static readonly byte[] MOVE_CURSER_DOWN = new byte[] { 0x1B, 0x5B, 0x42 };
        public event EventHandler TerminalClickRecive;
        DummyInstrument test;
        #endregion

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
        public bool IsConfirmed { get; private set; }
        #endregion
        BaseConnections _baseConnection;
        #region ctor
        public ConnectionHandler() { }

        #endregion

        #region Methods
        public void Init()
        {
            test = new DummyInstrument();
            BSelection = test.GetB();
            MSelection = test.GetM(BSelection.First());
            test.Axis = new Dictionary<Axis, int>() { { Axis.X, 0 }, { Axis.Y, 0 } };
            TerminalParameters = Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalPreferences);
            if(TerminalParameters["ConnectionType"] == "IP")
                _baseConnection = new TCP();
            else if (TerminalParameters["ConnectionType"] == "RS232")
                _baseConnection = new RS232();
            //_baseConnection.CloseConnection();
        }

        //public void WriteToMusafonScreenFromGUI(string value)
        //{
        //    if (_mySerialPort != null && _mySerialPort.IsOpen)
        //    {
        //        _mySerialPort.Write(Environment.NewLine + value);
        //    }
        //}


        #endregion 
    }
}
