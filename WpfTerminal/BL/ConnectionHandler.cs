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
        private SerialPort mySerialPort;
        private readonly int _configStepSize;
        public event EventHandler TerminalClickRecive;
        #endregion

        #region Properties
        //BG  typo - Succeeded
        public bool ConnectionSucceded { get; set; }
        public int TerminalStepSize { get; set; }
        public Dictionary<string, string> TerminalParameters { get; set; }
        #endregion

        #region ctor
        public ConnectionHandler()
        {
            if (!int.TryParse(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalData)["StepSize"], out _configStepSize))
                _configStepSize = 1;
            SettingRS232();
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
                mySerialPort.Write(Environment.NewLine + "Listening...");
                mySerialPort.DataReceived += DataReceivedHandler;

                ConnectionSucceded = true;
            }
            catch (Exception e)
            {
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
            System.Threading.Thread.Sleep(500);
            string indata = sp.ReadExisting();
            switch (indata.ToLower())
            {
                case "1":
                    {
                        WritToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["1"]);
                        break;
                    }
                case "2":
                    {
                        WritToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["2"]);
                        break;
                    }
                case "3":
                    {
                        WritToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["3"]);
                        break;
                    }
                case "4":
                    {
                        WritToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["4"]);
                        break;
                    }
                case "5":
                    {
                        WritToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["5"]);
                        break;
                    }
                case "6":
                    {
                        WritToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["6"]);
                        break;
                    }
                case "7":
                    {
                        WritToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["7"]);
                        break;
                    }
                case "8":
                    {
                        WritToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["8"]);
                        break;
                    }
                case "9":
                    {
                        WritToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["9"]);
                        break;
                    }
                case "0":
                    {
                        WritToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["0"]);
                        break;
                    }
                    //if want to use f1-f4 to move curser in screen
                case "a":
                case "b":
                case "c":
                case "d":
                    {
                        MoveCurser(indata.ToLower());
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
                case "a":
                    {
                            //BG - please put these in constants with a meaningful name
                            mySerialPort.Write(new byte[] { 0x1B, 0x5B, 0x41 },0,3);
                            break;

                    }
                    case "b":
                        {
                            mySerialPort.Write(new byte[] { 0x1B, 0x5B, 0x44 }, 0, 3);
                            break;

                        }
                    case "c":
                        {
                            mySerialPort.Write(new byte[] { 0x1B, 0x5B, 0x43 }, 0, 3);
                            break;

                        }
                    case "d":
                        {
                            mySerialPort.Write(new byte[] { 0x1B, 0x5B, 0x42 }, 0, 3);
                            break;

                        }
                }
            }
        }

        //BG - typo Write
        public void WritToTerminalScreen(string value)
        {
            if (mySerialPort != null && mySerialPort.IsOpen)
            {
                if (value.ToLower() == "s")
                {
                    TerminalStepSize++;
                    if (TerminalStepSize == _configStepSize)
                      mySerialPort.Write(Environment.NewLine + "Step Size: "+1);
                    else
                        mySerialPort.Write(Environment.NewLine + "Step Size: " + TerminalStepSize);
                }
                else
                {
                    mySerialPort.Write(Environment.NewLine + value);
                }
                TerminalClickRecive(value, null);
            }
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
                mySerialPort.WriteLine(Environment.NewLine+"Try Disconnect");
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
