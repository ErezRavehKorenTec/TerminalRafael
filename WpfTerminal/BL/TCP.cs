using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfTerminal.Enums;

namespace WpfTerminal.BL
{
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public String sb = string.Empty;
    }
    public class TCP : BaseConnections
    {
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private Socket tempSocket;
        private static event EventHandler _dataReceive;
        public TCP()
        {
            Init();
        }

        private void Init()
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(TerminalParameters["IP"]), 4001);
            tempSocket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            tempSocket.BeginConnect(endpoint, new AsyncCallback(ConnectCallback), tempSocket);
            connectDone.WaitOne();
            WriteToTerminalScreen(string.Empty);
            sendDone.WaitOne();
            _dataReceive += new EventHandler(OnDataReceive);
            ReciveData();
            receiveDone.WaitOne();
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public override void WriteToTerminalScreen(string input)
        {
            CurserLine = 4;
            byte[] value;
            ClearScreen();
            if (input.Equals(string.Empty))
            {
                value = Encoding.ASCII.GetBytes(FIRST_LINE_DISPLAY + BSelection[_bIndex].ToString() + Environment.NewLine + SECOND_LINE_DISPLAY + MSelection[_mIndex].ToString() + Environment.NewLine + THIRD_LINE_DISPLAY + SelectedStep.ToString() + Environment.NewLine + "Confirm ?");
                CurserLine = 4;
                tempSocket.BeginSend(value, 0, value.Length, 0, new AsyncCallback(SendCallback), tempSocket);
                AdjustCurserLocation();
                IsConfirmed = false;
            }
            else if (input.ToLower().Equals("confirm"))
            {
                value = Encoding.ASCII.GetBytes(FIRST_LINE_DISPLAY + BSelection[_bIndex].ToString() + Environment.NewLine + SECOND_LINE_DISPLAY + MSelection[_mIndex].ToString() + Environment.NewLine + THIRD_LINE_DISPLAY + SelectedStep.ToString() + Environment.NewLine);
                tempSocket.BeginSend(value, 0, value.Length, 0, new AsyncCallback(SendCallback), tempSocket);
                CurserLine = 4;

                IsConfirmed = true;
            }
            else
            {
                value = Encoding.ASCII.GetBytes(FIRST_LINE_DISPLAY + BSelection[_bIndex].ToString() + Environment.NewLine + SECOND_LINE_DISPLAY + MSelection[_mIndex].ToString() + Environment.NewLine + THIRD_LINE_DISPLAY + SelectedStep.ToString() + Environment.NewLine + input);
                tempSocket.BeginSend(value, 0, value.Length, 0, new AsyncCallback(SendCallback), tempSocket);
            }
            //if (TerminalClickRecive != null)
            //    TerminalClickRecive(value, null);
        }
        private void ClearScreen()
        {
            byte[] value = Encoding.ASCII.GetBytes(Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine);
            tempSocket.BeginSend(value, 0, value.Length, 0, new AsyncCallback(SendCallback), tempSocket);
            CurserLine = 4;
        }

        public override void ReciveData()
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = tempSocket;
                // Begin receiving the data from the remote device.
                tempSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                string response = string.Empty;
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                    state.sb = (Encoding.ASCII.GetString(state.buffer, 0, bytesRead)).ToString();
                _dataReceive(state.sb.ToString(), null);
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                //receiveDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void OnDataReceive(object sender, EventArgs e)
        {
            DataReceivedHandler(sender.ToString());
        }

        private void DataReceivedHandler(string indata)
        {
            switch (indata.ToLower())
            {
                case "2":
                    {
                        if (IsConfirmed)
                        {
                            WriteToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["2"]);
                            base.test.Axis[Axis.Y]++;
                            base.test.Move(test.Axis, SelectedStep, MSelection[_mIndex], BSelection[_bIndex]);
                        }
                        break;
                    }
                case "4":
                    {
                        if (IsConfirmed)
                        {
                            WriteToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["4"]);
                            base.test.Axis[Axis.X]--;
                            base.test.Move(test.Axis, SelectedStep, MSelection[_mIndex], BSelection[_bIndex]);
                        }
                        break;
                    }
                case "6":
                    {
                        if (IsConfirmed)
                        {
                            WriteToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["6"]);
                            base.test.Axis[Axis.X]++;
                            base.test.Move(test.Axis, SelectedStep, MSelection[_mIndex], BSelection[_bIndex]);
                        }
                        break;
                    }
                case "8":
                    {
                        if (IsConfirmed)
                        {
                            WriteToTerminalScreen(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalButtons)["8"]);
                            base.test.Axis[Axis.Y]--;
                            base.test.Move(test.Axis, SelectedStep, MSelection[_mIndex], BSelection[_bIndex]);
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
            if (tempSocket != null)
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
                                    tempSocket.BeginSend(MOVE_CURSER_DOWN, 0, MOVE_CURSER_DOWN.Length, 0, new AsyncCallback(SendCallback), tempSocket);
                                }
                            }
                            else// (CurserLine > 1)
                            {
                                CurserLine--;
                                tempSocket.BeginSend(MOVE_CURSER_UP, 0, MOVE_CURSER_UP.Length, 0, new AsyncCallback(SendCallback), tempSocket);
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
                                    tempSocket.BeginSend(MOVE_CURSER_UP, 0, MOVE_CURSER_UP.Length, 0, new AsyncCallback(SendCallback), tempSocket);
                                }
                            }
                            else// (CurserLine < 4)
                            {
                                CurserLine++;
                                tempSocket.BeginSend(MOVE_CURSER_DOWN, 0, MOVE_CURSER_DOWN.Length, 0, new AsyncCallback(SendCallback), tempSocket);

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
        public override bool CloseConnection()
        {
            if (tempSocket != null)
            {
                tempSocket.Close();
                return true;
            }
            return false;

        }
    }
}
