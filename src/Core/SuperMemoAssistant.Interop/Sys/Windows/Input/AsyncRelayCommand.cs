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
// Created On:   2019/02/25 23:57
// Modified On:  2019/02/26 12:48
// Modified By:  Alexis

#endregion




using System;
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

    private bool _isExecuting;

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




    #region Methods Impl

    public bool CanExecute()
    {
      return !_isExecuting && (_canExecute?.Invoke() ?? true);
    }

    public async Task ExecuteAsync()
    {
      if (CanExecute())
        try
        {
          _isExecuting = true;
          await _execute();
        }
        finally
        {
          _isExecuting = false;
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

    #endregion




    #region Explicit implementations

    bool ICommand.CanExecute(object parameter)
    {
      return parameter != null && CanExecute();
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

    private bool _isExecuting;

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




    #region Methods Impl

    public bool CanExecute(T parameter)
    {
      return !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);
    }

    public async Task ExecuteAsync(T parameter)
    {
      if (CanExecute(parameter))
        try
        {
          _isExecuting = true;
          await _execute(parameter);
        }
        finally
        {
          _isExecuting = false;
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
