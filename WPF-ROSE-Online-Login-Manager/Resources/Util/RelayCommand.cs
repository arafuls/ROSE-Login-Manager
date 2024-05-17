using System.Windows.Input;



namespace ROSE_Online_Login_Manager.Resources.Util
{
    /// <summary>
    ///     A basic implementation of ICommand for use with WPF applications.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool>? _canExecute;



        /// <summary>
        ///     Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler? CanExecuteChanged;



        /// <summary>
        ///     Initializes a new instance of the RelayCommand class.
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked.</param>
        /// <param name="canExecute">The function that determines whether the command can execute in its current state.</param>
        public RelayCommand(Action<object> execute, Func<object, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }



        /// <summary>
        ///     Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter ?? throw new ArgumentNullException(nameof(parameter)));



        /// <summary>
        ///     Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object? parameter)
        {
            _execute?.Invoke(parameter);
        }



        /// <summary>
        ///     Raises the CanExecuteChanged event.
        /// </summary>
        protected virtual void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);



        /// <summary>
        ///     Raises the CanExecuteChanged event to indicate that the CanExecute method should be re-evaluated.
        /// </summary>
        public void RaiseCanExecuteChanged() => OnCanExecuteChanged();
    }
}
