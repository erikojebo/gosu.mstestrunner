using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gosu.MsTestRunner.UI.Mvvm
{
    public class AsyncDelegateCommand : ICommand
    {
        private readonly Func<bool> _canExecute;
        private readonly Func<Task> _execute;

        public AsyncDelegateCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute ?? (() => true);
        }

        public event EventHandler CanExecuteChanged = (sender, args) => {};

        public bool CanExecute(object parameter)
        {
            return _canExecute();
        }

        public async void Execute(object parameter)
        {
            await _execute();
        }

        public async Task ExecuteAsync()
        {
            await _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}