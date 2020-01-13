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
// Created On:   2020/01/11 15:17
// Modified On:  2020/01/11 15:27
// Modified By:  Alexis

#endregion




using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SuperMemoAssistant.Sys
{
  /// <summary>https://stackoverflow.com/questions/31875103/deserializing-a-dictionary-key-from-json-to-an-enum-in-net</summary>
  public class DictionaryWithEnumKeyJsonConverter : JsonConverter
  {
    #region Properties Impl - Public

    public override bool CanRead  => true;

    public override bool CanWrite => false;

    #endregion




    #region Methods Impl

    public override bool CanConvert(Type objectType)
    {
      return true;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      throw new NotSupportedException();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      if (reader.TokenType == JsonToken.Null) return null;

      // read to a dictionary with string key
      var genericArguments           = objectType.GetGenericArguments();
      var keyType                    = genericArguments[0];
      var valueType                  = genericArguments[1];

      var intermediateDictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), valueType);
      var intermediateDictionary     = (IDictionary)Activator.CreateInstance(intermediateDictionaryType);

      serializer.Populate(reader, intermediateDictionary);

      // convert to a dictionary with enum key
      var finalDictionary = (IDictionary)Activator.CreateInstance(objectType);

      foreach (DictionaryEntry pair in intermediateDictionary)
        finalDictionary.Add(ToEnum(keyType, pair.Key.ToString()), pair.Value);

      return finalDictionary;
    }

    #endregion




    #region Methods

    private static object ToEnum(Type enumType, string str)
    {
      return Enum.Parse(enumType, str);
    }

    #endregion
  }
}
