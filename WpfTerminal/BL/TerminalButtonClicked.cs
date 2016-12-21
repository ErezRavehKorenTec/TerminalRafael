using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfTerminal.BL
{
    //BG - sIf this is obsolete just delete it
    [Obsolete]
    public class TerminalButtonClicked
    {
        //public event EventHandler onMusafonButtonClicked;
        ConnectionHandler _conn = null;
        public TerminalButtonClicked(ConnectionHandler connection)
        {
            _conn = connection;
        }
    }
}
