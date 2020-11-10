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




namespace SuperMemoAssistant.Sys.Windows {
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Diagnostics.CodeAnalysis;
  using System.Linq;

  /// <summary>
  ///   Text and selection values that the user entered on your notification. The Key is the ID of the input, and the Value
  ///   is what the user entered.
  /// </summary>
  [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "<Pending>")]
  [Serializable]
  public class NotificationUserInput : IReadOnlyDictionary<string, string>
  {
    #region Properties & Fields - Non-Public

    private readonly NotificationActivator.NotificationUserInputData[] _data;

    #endregion




    #region Constructors

    internal NotificationUserInput(NotificationActivator.NotificationUserInputData[] data)
    {
      _data = data;
    }

    #endregion




    #region Properties Impl - Public

    public int Count => _data.Length;

    public string this[string key] => _data.First(i => i.Key == key).Value;

    public IEnumerable<string> Keys => _data.Select(i => i.Key);

    public IEnumerable<string> Values => _data.Select(i => i.Value);

    #endregion




    #region Methods Impl

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
      return _data.Select(i => new KeyValuePair<string, string>(i.Key, i.Value)).GetEnumerator();
    }

    public bool ContainsKey(string key)
    {
      return _data.Any(i => i.Key == key);
    }

    public bool TryGetValue(string key, out string value)
    {
      foreach (var item in _data)
        if (item.Key == key)
        {
          value = item.Value;
          return true;
        }

      value = null;
      return false;
    }

    #endregion
  }
}
