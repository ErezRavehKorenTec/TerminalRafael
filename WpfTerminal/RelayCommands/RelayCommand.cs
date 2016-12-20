using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfTerminal.RelayCommands
{
    public class RelayCommand : ICommand
    {
        private Predicate<object> _canExecute;
        private Action<object> _execute;
        private ICommand rightClickCommand;

        public RelayCommand(Action<object> execute) : this(execute, param=> true)
        {
        }
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            if (canExecute == null)
            {
                throw new ArgumentNullException("canExecute");
            }

            this._execute = execute;
            this._canExecute = canExecute;
        }

        public RelayCommand(ICommand rightClickCommand)
        {
            this.rightClickCommand = rightClickCommand;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;
            return _canExecute((object)parameter);
        }
        public void Execute(object parameter)
        {
            _execute((object)parameter);
        }
        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}
