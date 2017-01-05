using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfTerminal.Enums;

namespace WpfTerminal.BL
{
    public class RS232 : BaseConnections
    {
        private SerialPort _mySerialPort;
        public RS232()
        {
            Init();
        }

        private void Init()
        {
            try
            {

                //TerminalParameters = Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalPreferences);
                _mySerialPort = new SerialPort(base.TerminalParameters["Port"]);
                _mySerialPort.BaudRate = Int16.Parse(base.TerminalParameters["BaudRate"]);
                _mySerialPort.DataBits = Int16.Parse(base.TerminalParameters["DataBits"]);


                Parity _tempParity;
                if (Enum.TryParse<Parity>(base.TerminalParameters["Parity"], out _tempParity))
                {
                    _mySerialPort.Parity = _tempParity;
                }
                else
                {
                    _mySerialPort.Parity = Parity.None;
                }
                StopBits _tempStopBits;
                if (Enum.TryParse<StopBits>(base.TerminalParameters["StopBits"], out _tempStopBits))
                {
                    _mySerialPort.StopBits = _tempStopBits;
                }
                else
                {
                    _mySerialPort.StopBits = StopBits.None;
                }


                Handshake _tempHandShake;
                if (Enum.TryParse<Handshake>(base.TerminalParameters["Handshake"], out _tempHandShake)) //SetHandShake(TerminalParameters["Handshake"]);
                {
                    _mySerialPort.Handshake = _tempHandShake;
                }
                else
                {
                    _mySerialPort.Handshake = Handshake.None;
                }
                _mySerialPort.ReadTimeout = Int16.Parse(base.TerminalParameters["ReadTimeout"]);
                _mySerialPort.WriteTimeout = Int16.Parse(base.TerminalParameters["WriteTimeout"]);

                _mySerialPort.DtrEnable = bool.Parse(base.TerminalParameters["DtrEnable"]);
                _mySerialPort.RtsEnable = bool.Parse(base.TerminalParameters["RtsEnable"]);
                _mySerialPort.Open();
                WriteToTerminalScreen(string.Empty);
                _mySerialPort.DataReceived += DataReceivedHandler;

                ConnectionSucceded = true;
            }
            catch (Exception e)
            {
                CloseConnection();
                ConnectionSucceded = false;
                string exceptionMessage = string.Empty;
                exceptionMessage = e.Message.Contains('(') ? e.Message : e.Message + " (ConnectionHandler)";
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
                        if (IsConfirmed)
                        {
                            WriteToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["2"]);
                            base.test.Axis[Axis.Y]++;
                            test.Move(test.Axis, SelectedStep, MSelection[_mIndex], BSelection[_bIndex]);
                        }
                        break;
                    }
                case "4":
                    {
                        if (IsConfirmed)
                        {
                            WriteToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["4"]);
                            test.Axis[Axis.X]--;
                            test.Move(test.Axis, SelectedStep, MSelection[_mIndex], BSelection[_bIndex]);
                        }
                        break;
                    }
                case "6":
                    {
                        if (IsConfirmed)
                        {
                            WriteToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["6"]);
                            test.Axis[Axis.X]++;
                            test.Move(test.Axis, SelectedStep, MSelection[_mIndex], BSelection[_bIndex]);
                        }
                        break;
                    }
                case "8":
                    {
                        if (IsConfirmed)
                        {
                            WriteToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["8"]);
                            test.Axis[Axis.Y]--;
                            test.Move(test.Axis, SelectedStep, MSelection[_mIndex], BSelection[_bIndex]);
                        }
                        break;
                    }
                //curser move on screen
                case "a":
                case "b":
                case "c":
                case "d":
                    {
                        MoveCursor(indata.ToLower());
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
        private void MoveCursor(string v)
        {
            if (_mySerialPort != null && _mySerialPort.IsOpen)
            {
                switch (v)
                {
                    //Move Up
                    case "a":
                        {
                            //if first row and press up go to last row
                            if (CurserLine == 1)
                            {
                                CurserLine = 4;
                                for (int i = 0; i < 4; i++)
                                {
                                    _mySerialPort.Write(MOVE_CURSER_DOWN, 0, 3);
                                }
                            }
                            else// (CurserLine > 1)
                            {
                                CurserLine--;
                                _mySerialPort.Write(MOVE_CURSER_UP, 0, 3);
                            }
                            LastCurserLocation = CurserLine;
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
                                        LastCurserLocation = CurserLine;
                                        ChangeLineSelection(1, v == "b" ? "left" : "right");
                                        break;
                                    }
                                case 2:
                                    {
                                        LastCurserLocation = CurserLine;
                                        ChangeLineSelection(2, v == "b" ? "left" : "right");
                                        break;
                                    }
                                case 3:
                                    {
                                        LastCurserLocation = CurserLine;
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
                            if (CurserLine == 4)
                            {
                                CurserLine = 1;
                                for (int i = 0; i < 4; i++)
                                {
                                    _mySerialPort.Write(MOVE_CURSER_UP, 0, 3);
                                }
                            }
                            else// (CurserLine < 4)
                            {
                                CurserLine++;
                                _mySerialPort.Write(MOVE_CURSER_DOWN, 0, 3);
                            }
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

                            if (_bIndex == 0)
                            {
                                _bIndex = BSelection.Count - 1;
                                //need to change m list according to b selection
                            }
                            else
                                _bIndex--;
                            //need to change m list according to b selection
                            break;
                        }
                    case 2:
                        {
                            if (_mIndex == 0)
                            {
                                _mIndex = MSelection.Count - 1;
                            }
                            else
                                _mIndex--;
                            break;
                        }
                    case 3:
                        {
                            if (SelectedStep == (StepSize)0)
                                SelectedStep = (StepSize)1;
                            else
                                SelectedStep = (StepSize)0;
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
                            if (_bIndex == BSelection.Count - 1)
                            {
                                _bIndex = 0;
                                //need to change m list according to b selection
                            }
                            else
                                _bIndex++;
                            //need to change m list according to b selection
                            break;
                        }
                    case 2:
                        {
                            if (_mIndex == MSelection.Count - 1)
                            {
                                _mIndex = 0;
                            }
                            else
                                _mIndex++;
                            break;
                        }
                    case 3:
                        {
                            if (SelectedStep == (StepSize)0)
                                SelectedStep = (StepSize)1;
                            else
                                SelectedStep = (StepSize)0;
                            break;
                        }
                }
            }
            WriteToTerminalScreen(string.Empty);
        }
        public override void WriteToTerminalScreen(string input)
        {
            string value;
            ClearScreen();
            if (input.Equals(string.Empty))
            {
                value = FIRST_LINE_DISPLAY + BSelection[_bIndex].ToString() + Environment.NewLine + SECOND_LINE_DISPLAY + MSelection[_mIndex].ToString() + Environment.NewLine + THIRD_LINE_DISPLAY + SelectedStep.ToString() + Environment.NewLine + "Confirm ?";
                CurserLine = 4;
                _mySerialPort.Write(value);
                AdjustCurserLocation();
                IsConfirmed = false;
            }
            else if (input.ToLower().Equals("confirm"))
            {
                value = FIRST_LINE_DISPLAY + BSelection[_bIndex].ToString() + Environment.NewLine + SECOND_LINE_DISPLAY + MSelection[_mIndex].ToString() + Environment.NewLine + THIRD_LINE_DISPLAY + SelectedStep.ToString() + Environment.NewLine;
                _mySerialPort.Write(value);
                CurserLine = 4;
                IsConfirmed = true;
            }
            else
            {
                value = FIRST_LINE_DISPLAY + BSelection[_bIndex].ToString() + Environment.NewLine + SECOND_LINE_DISPLAY + MSelection[_mIndex].ToString() + Environment.NewLine + THIRD_LINE_DISPLAY + SelectedStep.ToString() + Environment.NewLine + input;
                _mySerialPort.Write(value);
            }
            //if (TerminalClickRecive != null)
            //    TerminalClickRecive(value, null);
        }
        private void AdjustCurserLocation()
        {
            switch (LastCurserLocation)
            {
                case 1:
                    {
                        MoveCursor("a");
                        MoveCursor("a");
                        MoveCursor("a");
                        break;
                    }
                case 2:
                    {
                        MoveCursor("a");
                        MoveCursor("a");
                        break;
                    }
                case 3:
                    {
                        MoveCursor("a");
                        break;
                    }
                default:
                    break;
            }

        }
        private void ClearScreen()
        {
            _mySerialPort.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine);
            CurserLine = 4;
        }
        public override bool CloseConnection()
        {
            if (_mySerialPort!= null && _mySerialPort.IsOpen)
            {
                CurserLine = 4;
                ClearScreen();
                _mySerialPort.Write(Environment.NewLine + "Disconnect");
                _mySerialPort.Close();
            }
            return true;
        }
    }
}
