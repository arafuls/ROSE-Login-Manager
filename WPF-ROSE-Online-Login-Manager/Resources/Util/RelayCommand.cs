using System;
using System.Windows.Input;

namespace ROSE_Online_Login_Manager.Resources.Util
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public RelayCommand(Action<object> execute, Func<object, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter ?? throw new ArgumentNullException(nameof(parameter)));

        public void Execute(object? parameter)
        {
            _execute?.Invoke(parameter);
        }

        protected virtual void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void RaiseCanExecuteChanged() => OnCanExecuteChanged();
    }
}
