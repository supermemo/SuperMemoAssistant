#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Modified On:  2020/02/28 23:47
// Modified By:  Alexis

#endregion




using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using SuperMemoAssistant.Extensions;

namespace SuperMemoAssistant.Sys.Windows.Input
{
  public class AsyncRelayCommand : IAsyncCommand
  {
    #region Properties & Fields - Non-Public

    private readonly Func<bool>        _canExecute;
    private readonly Action<Exception> _errorHandler;
    private readonly Func<Task>        _execute;

    #endregion




    #region Constructors

    public AsyncRelayCommand(
      Func<Task>        execute,
      Func<bool>        canExecute   = null,
      Action<Exception> errorHandler = null)
    {
      _execute      = execute;
      _canExecute   = canExecute;
      _errorHandler = errorHandler;
    }

    #endregion




    #region Properties Impl - Public

    public bool IsExecuting { get; private set; }

    #endregion




    #region Methods Impl

    public bool CanExecute()
    {
      return !IsExecuting && (_canExecute?.Invoke() ?? true);
    }

    public async Task ExecuteAsync()
    {
      if (CanExecute())
        try
        {
          IsExecuting = true;
          await _execute();
        }
        finally
        {
          IsExecuting = false;
        }

      RaiseCanExecuteChanged();
    }

    #endregion




    #region Methods

    public void RaiseCanExecuteChanged()
    {
      CommandManager.InvalidateRequerySuggested(); //?.Invoke(this, EventArgs.Empty);
    }

    #endregion




    #region Events

    /// <summary>Occurs when changes occur that affect whether or not the command should execute.</summary>
    public event EventHandler CanExecuteChanged
    {
      add => CommandManager.RequerySuggested += value;
      remove => CommandManager.RequerySuggested -= value;
    }


    /// <inheritdoc />
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion




    #region Explicit implementations

    bool ICommand.CanExecute(object parameter)
    {
      return CanExecute();
    }

    void ICommand.Execute(object parameter)
    {
      ExecuteAsync().RunAsync(_errorHandler);
    }

    #endregion
  }

  public class AsyncRelayCommand<T> : IAsyncCommand<T>
  {
    #region Properties & Fields - Non-Public

    private readonly Func<T, bool>     _canExecute;
    private readonly Action<Exception> _errorHandler;
    private readonly Func<T, Task>     _execute;

    #endregion




    #region Constructors

    public AsyncRelayCommand(Func<T, Task>     execute,
                             Func<T, bool>     canExecute   = null,
                             Action<Exception> errorHandler = null)
    {
      _execute      = execute;
      _canExecute   = canExecute;
      _errorHandler = errorHandler;
    }

    #endregion




    #region Properties Impl - Public

    public bool IsExecuting { get; private set; }

    #endregion




    #region Methods Impl

    public bool CanExecute(T parameter)
    {
      return !IsExecuting && (_canExecute?.Invoke(parameter) ?? true);
    }

    public async Task ExecuteAsync(T parameter)
    {
      if (CanExecute(parameter))
        try
        {
          IsExecuting = true;
          await _execute(parameter);
        }
        finally
        {
          IsExecuting = false;
        }

      RaiseCanExecuteChanged();
    }

    #endregion




    #region Methods

    public void RaiseCanExecuteChanged()
    {
      CommandManager.InvalidateRequerySuggested(); //?.Invoke(this, EventArgs.Empty);
    }

    #endregion




    #region Events

    /// <summary>Occurs when changes occur that affect whether or not the command should execute.</summary>
    public event EventHandler CanExecuteChanged
    {
      add => CommandManager.RequerySuggested += value;
      remove => CommandManager.RequerySuggested -= value;
    }


    /// <inheritdoc />
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion




    #region Explicit implementations

    bool ICommand.CanExecute(object parameter)
    {
      return parameter != null && CanExecute((T)parameter);
    }

    void ICommand.Execute(object parameter)
    {
      ExecuteAsync((T)parameter).RunAsync(_errorHandler);
    }

    #endregion
  }
}
