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

#endregion




namespace SuperMemoAssistant.SuperMemo.Hooks
{
  using System;
  using System.Collections.Generic;
  using SMA.Hooks.Services;

  partial class SMHookEngine
  {
    #region Properties & Fields - Non-Public

    private List<string>     IOTargetFilePaths { get; } = new List<string>();
    private List<ISMAHookIO> IOCallbacks       { get; } = new List<ISMAHookIO>();

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public IEnumerable<string> GetTargetFilePaths()
    {
      return IOTargetFilePaths;
    }

    /// <inheritdoc />
    public void OnFileCreate(string filePath,
                             IntPtr fileHandle)
    {
      foreach (var callback in IOCallbacks)
        callback.OnFileCreate(filePath,
                              fileHandle);
    }

    /// <inheritdoc />
    public void OnFileSeek(IntPtr fileHandle,
                           UInt32 position)
    {
      foreach (var callback in IOCallbacks)
        callback.OnFileSeek(fileHandle,
                            position);
    }

    /// <inheritdoc />
    public void OnFileWrite(IntPtr fileHandle,
                            Byte[] buffer,
                            UInt32 count)
    {
      foreach (var callback in IOCallbacks)
        callback.OnFileWrite(fileHandle,
                             buffer,
                             count);
    }

    /// <inheritdoc />
    public void OnFileClose(IntPtr fileHandle)
    {
      foreach (var callback in IOCallbacks)
        callback.OnFileClose(fileHandle);
    }

    #endregion
  }
}
