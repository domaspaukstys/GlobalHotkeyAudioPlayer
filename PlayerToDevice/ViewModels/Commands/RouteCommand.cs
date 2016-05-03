using System;
using System.Windows.Input;

namespace PlayerToDevice.ViewModels.Commands
{
    public class RouteCommand : ICommand
    {
        private readonly Func<object, bool> _canExecute;
        private readonly Action<object> _execute;

        public RouteCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _execute != null && (_canExecute == null || _canExecute.Invoke(parameter));
        }

        public void Execute(object parameter)
        {
            _execute?.Invoke(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}