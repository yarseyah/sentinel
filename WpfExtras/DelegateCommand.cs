namespace WpfExtras
{
    using System;
    using System.Windows.Input;

    public class DelegateCommand : ICommand
    {
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

        private Predicate<object> CanExecutePredicate { get; set; }

        private Action<object> ExecuteAction { get; set; }

        public bool CanExecute(object parameter)
        {
            return CanExecutePredicate == null || CanExecutePredicate.Invoke(parameter);
        }

        public void Execute(object parameter)
        {
            ExecuteAction.Invoke(parameter);
        }
    }
}