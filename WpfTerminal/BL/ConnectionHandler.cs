using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfTerminal.Enums;

namespace WpfTerminal.BL
{
    public class ConnectionHandler
    {
        #region params

        //BG - both variables should be capitalized
        public const string _firstlinedisplay = "B: ";
        public const string _secondlinedisplay = "M: ";
        public const string _thirdlinedisplay = "Step: ";

        private SerialPort mySerialPort;
        //private readonly int _configStepSize;
        public event EventHandler TerminalClickRecive;
        #endregion

        #region Properties

        private int _curserLine = 4;
        public int CurserLine
        {
            get { return _curserLine; }
            set { _curserLine = value; }
        }

        private Enums.BSelection _bSelection = (Enums.BSelection)1;

        public Enums.BSelection SelectedB
        {
            get { return _bSelection; }
            set { _bSelection = value; }
        }
        private Enums.MSelection _mSelection = (Enums.MSelection)1;

        public Enums.MSelection SelectedM
        {
            get { return _mSelection; }
            set { _mSelection = value; }
        }
        private Step _stepSelection = (Enums.Step)1;

        public Step SelectedStep
        {
            get { return _stepSelection; }
            set { _stepSelection = value; }
        }
        public bool ConnectionSucceded { get; set; }
        public int TerminalStepSize { get; set; }
        public Dictionary<string, string> TerminalParameters { get; set; }
        #endregion

        #region ctor
        public ConnectionHandler()
        {
            SettingRS232();
            DummyInstrument test = new DummyInstrument();
        }
        #endregion

        //BG - I think the name of the method should be something like InitRS232Connection
        #region Methods
        private void SettingRS232()
        {
            try
            {
                TerminalParameters = Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalPreferences);

                mySerialPort = new SerialPort(TerminalParameters["Port"]);
                mySerialPort.BaudRate = Int16.Parse(TerminalParameters["BaudRate"]);
                mySerialPort.DataBits = Int16.Parse(TerminalParameters["DataBits"]);
                mySerialPort.Parity = SetParity(TerminalParameters["Parity"]);
                mySerialPort.StopBits = SetStopBit(TerminalParameters["StopBits"]);



                mySerialPort.Handshake = SetHS(TerminalParameters["Handshake"]);
                mySerialPort.ReadTimeout = Int16.Parse(TerminalParameters["ReadTimeout"]);
                mySerialPort.WriteTimeout = Int16.Parse(TerminalParameters["WriteTimeout"]);

                mySerialPort.DtrEnable = bool.Parse(TerminalParameters["DtrEnable"]);
                mySerialPort.RtsEnable = bool.Parse(TerminalParameters["RtsEnable"]);
                mySerialPort.Open();
                WriteToTerminalScreen(string.Empty);
                mySerialPort.DataReceived += DataReceivedHandler;

                ConnectionSucceded = true;
            }
            catch (Exception e)
            {
                Disconnect();
                ConnectionSucceded = false;
                string exceptionMessage = string.Empty;
                exceptionMessage = e.Message.Contains('(') ? e.Message : e.Message + " (ConnectionHandler)";

                //BG - rethrowing exception here is a bad idea we don't want the system to crush when the terminal couldn't connect (you also calling this in a constructor)
                // better way is to return bool and the caller is free to ignore it
                throw new Exception(exceptionMessage);
            }

        }

        //BG - Try to avoid acronyms in method names
        //BG -  Anway this method can be substituted by Enum.Parse
        private Handshake SetHS(string v)
        {
            switch (v.ToLower())
            {
                case "none":
                    return Handshake.None;
                case "requesttosend":
                    return Handshake.RequestToSend;
                case "requesttosendxonxoff":
                    return Handshake.RequestToSendXOnXOff;
                case "xonxoff":
                    return Handshake.XOnXOff;
                default:
                    return Handshake.None;
            }
        }

        //BG - same thing here
        private StopBits SetStopBit(string v)
        {
            switch (v.ToLower())
            {
                case "none":
                    return StopBits.None;
                case "one":
                    return StopBits.One;
                case "onepointfive":
                    return StopBits.OnePointFive;
                case "two":
                    return StopBits.Two;
                default:
                    return StopBits.None;
            }
        }
        //BG - same thing here
        private Parity SetParity(string v)
        {
            switch (v.ToLower())
            {
                case "even":
                    return Parity.Even;
                case "mark":
                    return Parity.Mark;
                case "none":
                    return Parity.None;
                case "odd":
                    return Parity.Odd;
                case "space":
                    return Parity.Space;
                default:
                    return Parity.None;
            }
        }


        public void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            System.Threading.Thread.Sleep(100);
            string indata = sp.ReadExisting();
            switch (indata.ToLower())
            {
                case "2":
                    {
                        WriteToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["2"]);
                        break;
                    }
                case "4":
                    {
                        WriteToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["4"]);
                        break;
                    }
                case "6":
                    {
                        WriteToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["6"]);
                        break;
                    }
                case "8":
                    {
                        WriteToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["8"]);
                        break;
                    }
                //curser move on screen
                case "a":
                case "b":
                case "c":
                case "d":
                    {
                        MoveCurser(indata.ToLower());
                        break;
                    }
                //confirm selection
                case "+":
                    {
                        WriteToTerminalScreen("confirm");
                        break;

                    }
                default:
                    {
                        break;
                    }
            }
        }

        //BG - typo - Cursor
        private void MoveCurser(string v)
        {
            if (mySerialPort != null && mySerialPort.IsOpen)
            {
                switch (v)
                {
                    //Move Up
                    case "a":
                        {
                            if (CurserLine > 1)
                                CurserLine--;
                            mySerialPort.Write(new byte[] { 0x1B, 0x5B, 0x41 }, 0, 3);
                            break;

                        }
                    //Selection Left/Right
                    case "b":
                    case "c":
                        {
                            switch (CurserLine)
                            {
                                case 1:
                                    {
                                        ChangeLineSelection(1, v == "b" ? "left" : "right");
                                        break;
                                    }
                                case 2:
                                    {
                                        ChangeLineSelection(2, v == "b" ? "left" : "right");
                                        break;
                                    }
                                case 3:
                                    {
                                        ChangeLineSelection(3, v == "b" ? "left" : "right");
                                        break;
                                    }
                                default:
                                    break;
                            }
                            break;
                        }
                    //Move Down
                    case "d":
                        {
                            if (CurserLine < 5)
                                CurserLine++;
                            mySerialPort.Write(new byte[] { 0x1B, 0x5B, 0x42 }, 0, 3);
                            break;

                        }
                }
            }
        }

        private void ChangeLineSelection(int line, string side)
        {
            if (side.ToLower().Equals("left"))
            {
                switch (line)
                {
                    case 1:
                        {
                            if (SelectedB == (BSelection)1)
                            {
                                SelectedB = Enum.GetValues(typeof(BSelection)).Cast<BSelection>().Last();
                            }
                            else
                                SelectedB--;
                            break;
                        }
                    case 2:
                        {
                            if (SelectedM == (MSelection)1)
                            {
                                SelectedM = Enum.GetValues(typeof(MSelection)).Cast<MSelection>().Last();
                            }
                            else
                                SelectedM--;
                            break;
                        }
                    case 3:
                        {
                            if (SelectedStep == (Step)1)
                                SelectedStep = (Step)2;
                            else
                                SelectedStep = (Step)1;
                            break;
                        }
                }
            }
            else
            {
                switch (line)
                {
                    case 1:
                        {
                            if (SelectedB == Enum.GetValues(typeof(BSelection)).Cast<BSelection>().Last())
                            {
                                SelectedB = Enum.GetValues(typeof(BSelection)).Cast<BSelection>().First();
                            }
                            else
                                SelectedB++;
                            break;
                        }
                    case 2:
                        {
                            if (SelectedM == Enum.GetValues(typeof(MSelection)).Cast<MSelection>().Last())
                            {
                                SelectedM = Enum.GetValues(typeof(MSelection)).Cast<MSelection>().First();
                            }
                            else
                                SelectedM++;
                            break;
                        }
                    case 3:
                        {
                            if (SelectedStep == (Step)1)
                                SelectedStep = (Step)2;
                            else
                                SelectedStep = (Step)1;
                            break;
                        }
                }
            }
            WriteToTerminalScreen(string.Empty);
            // _firstlinedisplay + SelectedB.ToString() + Environment.NewLine + _secondlinedisplay + SelectedM.ToString() + Environment.NewLine + _thirdlinedisplay + SelectedStep.ToString()+Environment.NewLine);
        }
        private void WriteToTerminalScreen(string input)
        {
            string value;
            ClearScreen();
            if (input.Equals(string.Empty))
            {
                value = _firstlinedisplay + SelectedB.ToString() + Environment.NewLine + _secondlinedisplay + SelectedM.ToString() + Environment.NewLine + _thirdlinedisplay + SelectedStep.ToString() + Environment.NewLine + "Confirm ?";
                mySerialPort.Write(value);
            }
            else if (input.ToLower().Equals("confirm"))
            {
                value = _firstlinedisplay + SelectedB.ToString() + Environment.NewLine + _secondlinedisplay + SelectedM.ToString() + Environment.NewLine + _thirdlinedisplay + SelectedStep.ToString() + Environment.NewLine;
                mySerialPort.Write(value);
            }
            else
            {
                value = _firstlinedisplay + SelectedB.ToString() + Environment.NewLine + _secondlinedisplay + SelectedM.ToString() + Environment.NewLine + _thirdlinedisplay + SelectedStep.ToString() + Environment.NewLine + input;
                mySerialPort.Write(value);
            }
            CurserLine = 4;
            if(TerminalClickRecive!=null)
                TerminalClickRecive(value, null);
        }

        private void ClearScreen()
        {
            mySerialPort.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine);
            CurserLine = 4;
        }

        //BG - typo Write, plese avoid using hebrew in names - WriteToTerminalScreenFromUI
        public void WritToMusafonScreenFromGUI(string value)
        {
            if (mySerialPort != null && mySerialPort.IsOpen)
            {
                mySerialPort.Write(Environment.NewLine + value);
            }
        }

        public bool Disconnect()
        {
            try
            {
                ClearScreen();
                mySerialPort.Write(Environment.NewLine + "Disconnect");
                mySerialPort.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        #endregion 
    }
}
