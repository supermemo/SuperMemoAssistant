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
// Modified On:  2020/03/04 18:52
// Modified By:  Alexis

#endregion




using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SuperMemoAssistant.Sys.Windows.Input
{
  /// <summary>Extended command interface. Adds IsExecuting</summary>
  public interface ICommandEx : ICommand, INotifyPropertyChanged
  {
    /// <summary>Whether is command is currently executing</summary>
    bool IsExecuting { get; }
  }

  /// <summary>Extends <see cref="ICommand" /> to add async facilities</summary>
  public interface IAsyncCommand : ICommandEx
  {
    /// <summary>Execute the command asynchronously</summary>
    /// <returns>The task instance</returns>
    Task ExecuteAsync();

    /// <summary>
    ///   Checks whether the task can be executed, depending on its current execution status
    ///   and the user-provided CanExecute predicate
    /// </summary>
    /// <returns></returns>
    bool CanExecute();
  }

  /// <summary>Extends <see cref="ICommand" /> to add async facilities</summary>
  /// <typeparam name="T">The parameter type to pass to the command</typeparam>
  public interface IAsyncCommand<in T> : ICommandEx
  {
    /// <summary>Execute the command asynchronously</summary>
    /// <returns>The task instance</returns>
    Task ExecuteAsync(T parameter);

    /// <summary>
    ///   Checks whether the task can be executed, depending on its current execution status
    ///   and the user-provided CanExecute predicate
    /// </summary>
    /// <returns></returns>
    bool CanExecute(T parameter);
  }
}
