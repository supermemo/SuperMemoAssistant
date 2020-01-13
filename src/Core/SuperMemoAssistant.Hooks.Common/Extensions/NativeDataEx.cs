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
// Created On:   2020/01/11 18:36
// Modified On:  2020/01/11 20:58
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using Process.NET.Patterns;
using SuperMemoAssistant.SuperMemo;

namespace SuperMemoAssistant.Extensions
{
  public static class NativeDataEx
  {
    #region Methods

    public static IMemoryPattern GetMemoryPattern(this NativeData nativeData, NativeMethod nativeMethod)
    {
      if (nativeData.NativeCallPatterns.ContainsKey(nativeMethod))
      {
        var patternData = nativeData.NativeCallPatterns[nativeMethod];

        return new DwordCallPattern(patternData.Pattern, patternData.Offset);
      }

      if (nativeData.NativeDataPatterns.ContainsKey(nativeMethod))
      {
        var patternData = nativeData.NativeDataPatterns[nativeMethod];

        return new DwordDataPattern(patternData.Pattern, patternData.Offset);
      }

      if (nativeData.NativeFunctionPatterns.ContainsKey(nativeMethod))
        return new DwordFunctionPattern(nativeData.NativeFunctionPatterns[nativeMethod]);

      return null;
    }

    public static IEnumerable<(NativeMethod method, IMemoryPattern pattern)> GetAllMemoryPatterns(
      this NativeData nativeData)
    {
      foreach (var keyVal in nativeData.NativeCallPatterns)
      {
        var patternData = keyVal.Value;

        yield return (keyVal.Key, new DwordCallPattern(patternData.Pattern, patternData.Offset));
      }

      foreach (var keyVal in nativeData.NativeDataPatterns)
      {
        var patternData = keyVal.Value;

        yield return (keyVal.Key, new DwordDataPattern(patternData.Pattern, patternData.Offset));
      }

      foreach (var keyVal in nativeData.NativeFunctionPatterns)
        yield return (keyVal.Key, new DwordFunctionPattern(keyVal.Value));
    }

    #endregion
  }
}
