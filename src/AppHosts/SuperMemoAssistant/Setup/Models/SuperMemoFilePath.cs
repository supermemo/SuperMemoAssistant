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
// Modified On:  2020/03/10 22:30
// Modified By:  Alexis

#endregion




using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Extensions.System.IO;
using SuperMemoAssistant.Exceptions;
using SuperMemoAssistant.SMA.Configs;

namespace SuperMemoAssistant.Setup.Models
{
  public class SuperMemoFilePath : INotifyDataErrorInfo
  {
    #region Properties & Fields - Non-Public

    private readonly SMAException _ex;

    #endregion




    #region Constructors

    public SuperMemoFilePath(FilePath filePath, NativeDataCfg nativeDataCfg)
    {
      FilePath = filePath;

      HasErrors = SMA.Utils.SuperMemoFinder.CheckSuperMemoExecutable(nativeDataCfg, filePath, out _, out var ex) == false;
      _ex       = ex;
    }

    #endregion




    #region Properties & Fields - Public

    public FilePath FilePath { get; }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public bool HasErrors { get; }

    #endregion




    #region Methods Impl

    public override string ToString()
    {
      return FilePath?.FullPathWin ?? string.Empty;
    }

    /// <inheritdoc />
    public IEnumerable GetErrors(string propertyName)
    {
      return new List<string> { _ex.Message };
    }

    #endregion




    #region Methods

    public static implicit operator string(SuperMemoFilePath smFilePath)
    {
      return smFilePath.ToString();
    }

    #endregion




    #region Events

    /// <inheritdoc />
    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

    #endregion
  }
}
