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
// Created On:   2020/01/11 15:04
// Modified On:  2020/01/12 11:40
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SuperMemoAssistant.Sys;

namespace SuperMemoAssistant.SuperMemo
{
  [Serializable]
  public class NativeData
  {
    #region Constructors

    public NativeData()
    {
      SMVersion = new Version("0.0");

      Pointers               = new Dictionary<NativePointers, int>();
      NativeCallPatterns     = new Dictionary<NativeMethod, MemoryPatternWithOffset>();
      NativeDataPatterns     = new Dictionary<NativeMethod, MemoryPatternWithOffset>();
      NativeFunctionPatterns = new Dictionary<NativeMethod, string>();
    }

    #endregion




    #region Properties & Fields - Public

    [JsonConverter(typeof(VersionConverter))]
    public Version SMVersion { get; set; }

    [JsonConverter(typeof(DictionaryWithEnumKeyAndHexStringValueJsonConverter))]
    public Dictionary<NativePointers, int> Pointers { get; set; }

    [JsonConverter(typeof(DictionaryWithEnumKeyJsonConverter))]
    public Dictionary<NativeMethod, MemoryPatternWithOffset> NativeCallPatterns { get; set; }
    [JsonConverter(typeof(DictionaryWithEnumKeyJsonConverter))]
    public Dictionary<NativeMethod, MemoryPatternWithOffset> NativeDataPatterns { get; set; }
    [JsonConverter(typeof(DictionaryWithEnumKeyJsonConverter))]
    public Dictionary<NativeMethod, string> NativeFunctionPatterns { get; set; }

    #endregion
  }
}
