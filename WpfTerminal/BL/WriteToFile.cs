using System;
using System.IO;
using System.Windows.Forms;

namespace WpfTerminal.BL
{
    public class WriteToFile
    {
        private string[] _fileHeaderLines = { "Terminal Log", Environment.NewLine, "Logtime: ", DateTime.Now.ToString(), Environment.NewLine, Environment.NewLine, "---Log---", Environment.NewLine };
        private string path;
        public string StringLogToWrite { get; set; }
        public bool IsClearedLog { get; set; }


        public WriteToFile(string _logToWrite, bool _isClearLog)
        {
            StringLogToWrite = _logToWrite;
            IsClearedLog = _isClearLog;
            StartWriteToLog();
        }
        private void StartWriteToLog()
        {
            SaveFileDialog _saveDialog = new SaveFileDialog();
            _saveDialog.FileName = "TerminalLog.txt";
            _saveDialog.Filter = "txt files (*.txt)|";//*.txt|All files (*.*)|*.*
            _saveDialog.FilterIndex = 1;
            _saveDialog.OverwritePrompt = true;
            if (_saveDialog.ShowDialog() == DialogResult.OK)
            {
                path = Path.GetFullPath(_saveDialog.FileName);
                if (IsClearedLog)
                    path += DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString();
                File.WriteAllLines(path, _fileHeaderLines);
                File.WriteAllText(path, String.Concat(string.Join("", _fileHeaderLines), StringLogToWrite));
            }
        }
    }
}
