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

        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public bool CanExecute(object parameter)
        {
            return CanExecutePredicate == null || CanExecutePredicate.Invoke(parameter);
        }

        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public void Execute(object parameter)
        {
            ExecuteAction.Invoke(parameter);
        }
    }
}