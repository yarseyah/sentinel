namespace Sentinel.Support.Mvvm
{
    using System;
    using System.Windows.Input;

    public class DelegateCommand : ICommand
    {
        private Predicate<object> CanExecutePredicate { get; }

        private Action<object> ExecuteAction { get; }

        public DelegateCommand(Action<object> executeAction, Predicate<object> canExecute = null)
        {
            ExecuteAction = executeAction;
            CanExecutePredicate = canExecute;
        }

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
            return CanExecutePredicate?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            ExecuteAction.Invoke(parameter);
        }
    }
}