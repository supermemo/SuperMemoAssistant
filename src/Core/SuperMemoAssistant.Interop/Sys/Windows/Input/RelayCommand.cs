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
// Created On:   2019/03/02 18:29
// Modified On:  2019/04/14 00:53
// Modified By:  Alexis

#endregion




using System;
using System.Windows.Input;

namespace SuperMemoAssistant.Sys.Windows.Input
{
  public class RelayCommand : ICommand
  {
    #region Constructors

    /// <summary>Creates a new command.</summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    public RelayCommand(Action     execute,
                        Func<bool> canExecute = null)
    {
      _execute    = execute ?? throw new ArgumentNullException(nameof(execute));
      _canExecute = canExecute;
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




    #region Fields

    private readonly Action     _execute    = null;
    private readonly Func<bool> _canExecute = null;

    #endregion




    #region ICommand Members

    /// <summary>
    ///   Defines the method that determines whether the command can execute in its current
    ///   state.
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command.  If the command does not require data to be
    ///   passed, this object can be set to null.
    /// </param>
    /// <returns>true if this command can be executed; otherwise, false.</returns>
    public bool CanExecute(object parameter)
    {
      return _canExecute?.Invoke() ?? true;
    }

    /// <summary>Defines the method to be called when the command is invoked.</summary>
    /// <param name="parameter">
    ///   Data used by the command. If the command does not require data to be
    ///   passed, this object can be set to <see langword="null" />.
    /// </param>
    public void Execute(object parameter)
    {
      _execute();
    }

    #endregion
  }

  public class RelayCommand<T> : ICommand
  {
    #region Constructors

    /// <summary>Creates a new command.</summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    public RelayCommand(Action<T>    execute,
                        Predicate<T> canExecute = null)
    {
      _execute    = execute ?? throw new ArgumentNullException(nameof(execute));
      _canExecute = canExecute;
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




    #region Fields

    private readonly Action<T>    _execute    = null;
    private readonly Predicate<T> _canExecute = null;

    #endregion




    #region ICommand Members

    /// <summary>
    ///   Defines the method that determines whether the command can execute in its current
    ///   state.
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command.  If the command does not require data to be
    ///   passed, this object can be set to null.
    /// </param>
    /// <returns>true if this command can be executed; otherwise, false.</returns>
    public bool CanExecute(object parameter)
    {
      return parameter != null && (_canExecute?.Invoke((T)parameter) ?? true);
    }

    /// <summary>Defines the method to be called when the command is invoked.</summary>
    /// <param name="parameter">
    ///   Data used by the command. If the command does not require data to be
    ///   passed, this object can be set to <see langword="null" />.
    /// </param>
    public void Execute(object parameter)
    {
      _execute((T)parameter);
    }

    #endregion
  }
}
