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
// Created On:   2019/07/30 14:22
// Modified On:  2019/07/30 14:24
// Modified By:  Alexis

#endregion




using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.SuperMemo.Common.Registry.Files;

namespace SuperMemoAssistant.SuperMemo.Common.Extensions
{
  public static class SMCollectionEx
  {
    #region Methods

    public static string GetMemFilePath(
      this SMCollection       collection,
      IRegistryFileDescriptor fileDesc)
    {
      return collection.CombinePath(SMConst.Paths.RegistryFolder,
                                    fileDesc.MemFileName);
    }

    public static string GetRtxFilePath(
      this SMCollection       collection,
      IRegistryFileDescriptor fileDesc)
    {
      return collection.CombinePath(SMConst.Paths.RegistryFolder,
                                    fileDesc.RtxFileName);
    }

    public static string GetRtfFilePath(
      this SMCollection       collection,
      IRegistryFileDescriptor fileDesc)
    {
      return collection.CombinePath(SMConst.Paths.RegistryFolder,
                                    fileDesc.RtfFileName);
    }

    public static string GetInfoFilePath(
      this SMCollection collection,
      string            fileName)
    {
      return collection.CombinePath(SMConst.Paths.InfoFolder,
                                    fileName);
    }

    #endregion
  }
}
