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
// Created On:   2018/10/26 22:37
// Modified On:  2019/01/26 06:10
// Modified By:  Alexis

#endregion




using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace SuperMemoAssistant.Extensions
{
  public static class AssemblyEx
  {
    #region Methods

    public static Guid GetAssemblyGuid(Type typeInAssembly)
    {
      var assembly = typeInAssembly.Assembly;
      var guidAttr = assembly.GetCustomAttributes(typeof(GuidAttribute),
                                                  true);

      var guidStr = ((GuidAttribute)guidAttr.FirstOrDefault())?.Value
        ?? throw new NullReferenceException("GUID can't be null");

      return Guid.Parse(guidStr);
    }

    public static string GetAssemblyVersion(Type typeInAssembly)
    {
      var assembly = typeInAssembly.Assembly;
      var fvi      = FileVersionInfo.GetVersionInfo(assembly.Location);

      return fvi.FileVersion;
    }

    public static string GetAssemblyName(Type typeInAssembly)
    {
      return typeInAssembly.Assembly.FullName;
    }

    #endregion
  }
}
