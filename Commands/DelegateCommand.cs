using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Trades_Research.Commands
{
    public class DelegateCommand : ICommand
    {
        public DelegateCommand(DelegateFuncton function) 
        {
            _function = function;
        }
        public delegate void DelegateFuncton(object obj);

        public event EventHandler CanExecuteChanged;

        //===============================================================
        private DelegateFuncton _function;

        //===============================================================

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _function?.Invoke(parameter);
        }
    }
}
