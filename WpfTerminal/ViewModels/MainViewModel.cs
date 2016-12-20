using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfTerminal.BL;
using WpfTerminal.Enums;
using WpfTerminal.RelayCommands;

namespace WpfTerminal.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        #region params
        private String _logString = string.Empty;
        private String _terminalGUIString = string.Empty;
        private bool _isConnectionSucceded = false;
        public readonly int _configStepSize;
        //private TerminalButtonClicked terminalScreen;
        private ConnectionHandler _connectionHandler;
        public event PropertyChangedEventHandler PropertyChanged;

        #region ICommands
        private ICommand _clickConnect;
        private ICommand _clickDisConnect;
        private ICommand _clickClearScreen;
        private ICommand _close;
        private ICommand _export;
        private ICommand _terminalGUIClicked;
        #endregion
        
        #endregion
        
        #region Properties
        public int StepSize { get; set; }
        public bool IsConnectionSucceded
        {
            get
            {
                return _isConnectionSucceded;
            }
            set
            {
                _isConnectionSucceded = value;
            }
        }
        #endregion

        #region ctor
        public MainViewModel()
        {
            if (!int.TryParse(Configuration.ConfigurationHolder.GetInstance().GetValue(ConfigurationParameter.TerminalData)["StepSize"], out _configStepSize))
                _configStepSize = 1;
            ConnectCommand = new RelayCommand(ConnectDevice);
            DisConnectCommand = new RelayCommand(DisonnectDevice);
            ClearScreen = new RelayCommand(param => WriteToLog(string.Empty));
            ExitCommand = new RelayCommand(ExitProgram);
            ExportClickCommand = new RelayCommand(ExportClicked);
            TerminalGUIClickCommand = new RelayCommand(TerminalClickedFromGUI);

        }


        #endregion

        #region Bindable Properties
        public string TerminalGUIScreen
        {
            get { return _terminalGUIString; }
            set
            {
                _terminalGUIString = value;
                OnPropertyChange("TerminalGUIScreen");
            }
        }
        public string LogText
        {
            get { return _logString; }
            set
            {
                if (value == string.Empty)
                    _logString = string.Empty;
                else if (_logString == string.Empty)
                    _logString += value;
                else
                    _logString += Environment.NewLine + value;
                OnPropertyChange("LogText");
            }
        }
        #endregion

        #region Icommands Properties
        public ICommand ConnectCommand
        {
            get { return _clickConnect; }
            set { _clickConnect = value; }
        }
        public ICommand DisConnectCommand
        {
            get { return _clickDisConnect; }
            set { _clickDisConnect = value; }
        }
        public ICommand ExitCommand
        {
            get { return _close; }
            set { _close = value; }
        }

        public ICommand ClearScreen
        {
            get { return _clickClearScreen; }
            set { _clickClearScreen = value; }
        }

        public ICommand ExportClickCommand
        {
            get { return _export; }
            set { _export = value; }
        }
        public ICommand TerminalGUIClickCommand
        {
            get { return _terminalGUIClicked; }
            set { _terminalGUIClicked = value; }
        }
        #endregion

        #region Methods/Bind Execution
        private void ConnectDevice(object obj)
        {
            try
            {
                _connectionHandler = new ConnectionHandler();
                if (_connectionHandler.ConnectionSucceded == true)
                {
                    WriteToLog(StringsText.Connect_succeded + Environment.NewLine + StringsText.Listening);
                    _connectionHandler.TerminalClickRecive += new EventHandler(OnTerminalClicked);
                    //if want to move button click from terminal to new class
                    //terminalScreen = new TerminalButtonClicked(_connectionHandler);
                    IsConnectionSucceded = true;
                }
                else
                {
                    WriteToLog(StringsText.Connect_Unsucceded);
                    IsConnectionSucceded = false;
                }
            }
            catch (Exception e)
            {
                LogText = e.Message;
                OnPropertyChange("LogText");
            }
            finally
            {
                OnPropertyChange("IsConnectionSucceded");
            }
        }

        //Update Phisical Terminal according to GUI pressing
        private void TerminalClickedFromGUI(object obj)
        {
            TerminalClicked(obj, true);
        }
        //Update GUI according to phisical terminal click
        private void OnTerminalClicked(object sender, EventArgs e)
        {
            TerminalClicked(sender, false);
        }
        private void ExitProgram(object obj)
        {
            DisonnectDevice(null);
            if (LogText != string.Empty)
            {
                MessageBoxResult result = MessageBox.Show("Do you want to export log before close this window?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    ExportClicked(null);
                }

            }
            Application.Current.MainWindow.Close();
        }
        private void DisonnectDevice(object obj)
        {
            if (_connectionHandler.Disconnect())
            {
                WriteToLog(StringsText.Disconnect_succeded);
                IsConnectionSucceded = false;
                OnPropertyChange("IsConnectionSucceded");
            }
            else
            {
                WriteToLog(StringsText.Disconnect_unsucceded);
            }
        }
        private void ExportClicked(object obj)
        {
            new WriteToFile(LogText, false);
        }
        private void WriteToLog(string obj)
        {
            LogText = obj;
        }
        //two way update of GUI/Terminal
        private void TerminalClicked(object obj, bool isFromGUI)
        {
            string logMessage = string.Empty;
            switch (((string)obj).ToLower())
            {
                case "left":
                    {
                        if (StepSize == 0) StepSize++;
                        logMessage = ("Left Button Clicked, Move Left " + StepSize + " Levels");
                        TerminalGUIScreen = "Left";
                        break;
                    }
                case "right":
                    {
                        if (StepSize == 0) StepSize++;
                        logMessage = ("Right Button Clicked, Move Right " + StepSize + " Levels");
                        TerminalGUIScreen = "Right";
                        break;
                    }
                case "down":
                    {
                        if (StepSize == 0) StepSize++;
                        logMessage = ("Down Button Clicked, Move Down " + StepSize + " Levels");
                        TerminalGUIScreen = "Down";
                        break;
                    }
                case "up":
                    {
                        if (StepSize == 0) StepSize++;
                        logMessage = ("Up Button Clicked, Move Up " + StepSize + " Levels");
                        TerminalGUIScreen = "Up";
                        break;
                    }
                case "s":
                    {
                        //if there was update by musafon we need to write it to GUI and update proper Step size
                        if (_connectionHandler.TerminalStepSize > StepSize)
                            StepSize = _connectionHandler.TerminalStepSize;
                        //update GUI Step size only when GUI is Called method
                        if (isFromGUI)
                        {
                            StepSize++;
                        }
                        //
                        if (StepSize == _configStepSize)
                            StepSize = 0;
                        if (StepSize == 0)
                        {
                            logMessage = ("S Button Clicked, Number Of Step Size is:" + (StepSize + 1).ToString());
                            TerminalGUIScreen = "Step Size: " + (StepSize + 1);
                            _connectionHandler.TerminalStepSize = StepSize + 1;
                        }
                        else
                        {
                            logMessage = ("S Button Clicked, Number Of Step Size is:" + (StepSize).ToString());
                            TerminalGUIScreen = "Step Size: " + StepSize;
                            _connectionHandler.TerminalStepSize = StepSize;
                        }
                        break;
                    }
                case "m":
                    {
                        logMessage = ("M Button Clicked");
                        TerminalGUIScreen = "M";
                        break;
                    }
                case "b":
                    {
                        logMessage = ("B Button Clicked");
                        TerminalGUIScreen = "B";
                        break;
                    }

                default:
                    {
                        logMessage = ("Button Clicked is not defint,Please define key first");
                        TerminalGUIScreen = "??";
                        break;
                    }
            }

            if (isFromGUI)
            {
                WriteToLog("GUI:" + logMessage);
                _connectionHandler.WritToMusafonScreenFromGUI(TerminalGUIScreen);
            }
            else
                WriteToLog("Musafon:" + logMessage);
        }
        private void OnPropertyChange(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                this.PropertyChanged(this, e);
            }
        }
        #endregion
    }
}
