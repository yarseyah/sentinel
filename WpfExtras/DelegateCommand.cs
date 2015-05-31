using System;
using System.Windows.Input;

namespace WpfExtras
{
    public class DelegateCommand : ICommand
    {
        private readonly Predicate<object> canExecute;

        private readonly Action<object> executeAction;

        public DelegateCommand(Action<object> executeAction)
            : this(executeAction, null)
        {
        }

        public DelegateCommand(Action<object> executeAction, Predicate<object> canExecute)
        {
            this.executeAction = executeAction;
            this.canExecute = canExecute;
        }

        #region ICommand Members

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null ? true : canExecute.Invoke(parameter);
        }

        public void Execute(object parameter)
        {
            executeAction.Invoke(parameter);
        }

        #endregion
    }
}