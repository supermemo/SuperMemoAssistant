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
// Modified On:  2020/03/11 12:31
// Modified By:  Alexis

#endregion




using System.ComponentModel;

namespace SuperMemoAssistant.Setup
{
  /// <summary>Contract interface for setup screens</summary>
  public interface ISMASetupScreen : INotifyPropertyChanged
  {
    /// <summary>Whether this setup step is done</summary>
    bool IsSetup { get; }

    /// <summary>The title to display in the setup step list</summary>
    string ListTitle { get; }

    /// <summary>The title to display in the setup window title bar</summary>
    string WindowTitle { get; }

    /// <summary>Description of the current setup step</summary>
    string Description { get; }

    /// <summary>Called when this screen is displayed</summary>
    void OnDisplayed();

    /// <summary>
    ///   Called when this screen is about to be swapped out for the next one (or the end of
    ///   the setup process)
    /// </summary>
    void OnNext();
  }
}
