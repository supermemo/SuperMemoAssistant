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
// Modified On:  2020/03/12 22:07
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static System.Double;

namespace SuperMemoAssistant.Sys.IO
{
  public class IniValue
  {
    #region Constants & Statics

    private static readonly IniValue _default = new IniValue();
    public static IniValue Default => _default;

    #endregion




    #region Constructors

    public IniValue() {}

    public IniValue(object value)
    {
      var formattable = value as IFormattable;
      if (formattable != null)
        _value = formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture);
      else
        _value = value != null ? value.ToString() : null;
    }

    public IniValue(string value)
    {
      _value = value;
    }

    #endregion




    #region Properties & Fields - Public

    private readonly string _value;

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public override string ToString()
    {
      return _value;
    }

    #endregion




    #region Methods

    private static bool TryParseInt(string text, out int value)
    {
      if (int.TryParse(text,
                       System.Globalization.NumberStyles.Integer,
                       System.Globalization.CultureInfo.InvariantCulture,
                       out var res))
      {
        value = res;
        return true;
      }

      value = 0;
      return false;
    }

    private static bool TryParseDouble(string text, out double value)
    {
      if (TryParse(text,
                   System.Globalization.NumberStyles.Float,
                   System.Globalization.CultureInfo.InvariantCulture,
                   out var res))
      {
        value = res;
        return true;
      }

      value = NaN;
      return false;
    }

    public bool ToBool(bool valueIfInvalid = false)
    {
      bool res;
      if (TryConvertBool(out res))
        return res;

      return valueIfInvalid;
    }

    public bool TryConvertBool(out bool result)
    {
      if (_value == null)
      {
        result = default(bool);
        return false;
      }

      var boolStr = _value.Trim().ToLowerInvariant();
      if (boolStr == "true")
      {
        result = true;
        return true;
      }
      else if (boolStr == "false")
      {
        result = false;
        return true;
      }

      result = default(bool);
      return false;
    }

    public int ToInt(int valueIfInvalid = 0)
    {
      int res;
      if (TryConvertInt(out res))
        return res;

      return valueIfInvalid;
    }

    public bool TryConvertInt(out int result)
    {
      if (_value == null)
      {
        result = default(int);
        return false;
      }

      if (TryParseInt(_value.Trim(), out result))
        return true;

      return false;
    }

    public double ToDouble(double valueIfInvalid = 0)
    {
      double res;
      if (TryConvertDouble(out res))
        return res;

      return valueIfInvalid;
    }

    public bool TryConvertDouble(out double result)
    {
      if (_value == null)
      {
        result = default(double);
        return false;

        ;
      }

      if (TryParseDouble(_value.Trim(), out result))
        return true;

      return false;
    }

    public string GetString()
    {
      return GetString(true, false);
    }

    public string GetString(bool preserveWhitespace)
    {
      return GetString(true, preserveWhitespace);
    }

    public string GetString(bool allowOuterQuotes, bool preserveWhitespace)
    {
      if (_value == null)
        return "";

      var trimmed = _value.Trim();
      if (allowOuterQuotes && trimmed.Length >= 2 && trimmed[0] == '"' && trimmed[trimmed.Length - 1] == '"')
      {
        var inner = trimmed.Substring(1, trimmed.Length - 2);
        return preserveWhitespace ? inner : inner.Trim();
      }
      else
      {
        return preserveWhitespace ? _value : _value.Trim();
      }
    }

    public static implicit operator IniValue(byte o)
    {
      return new IniValue(o);
    }

    public static implicit operator IniValue(short o)
    {
      return new IniValue(o);
    }

    public static implicit operator IniValue(int o)
    {
      return new IniValue(o);
    }

    public static implicit operator IniValue(sbyte o)
    {
      return new IniValue(o);
    }

    public static implicit operator IniValue(ushort o)
    {
      return new IniValue(o);
    }

    public static implicit operator IniValue(uint o)
    {
      return new IniValue(o);
    }

    public static implicit operator IniValue(float o)
    {
      return new IniValue(o);
    }

    public static implicit operator IniValue(double o)
    {
      return new IniValue(o);
    }

    public static implicit operator IniValue(bool o)
    {
      return new IniValue(o);
    }

    public static implicit operator IniValue(string o)
    {
      return new IniValue(o);
    }

    #endregion
  }

  public class IniFile : IEnumerable<KeyValuePair<string, IniSection>>, IDictionary<string, IniSection>
  {
    #region Constants & Statics

    public static IEqualityComparer<string> DefaultComparer = new CaseInsensitiveStringComparer();

    #endregion




    #region Properties & Fields - Non-Public

    private readonly Dictionary<string, IniSection> sections;

    bool ICollection<KeyValuePair<string, IniSection>>.IsReadOnly
    {
      get { return ((IDictionary<string, IniSection>)sections).IsReadOnly; }
    }

    #endregion




    #region Constructors

    public IniFile()
      : this(DefaultComparer) { }

    public IniFile(IEqualityComparer<string> stringComparer)
    {
      StringComparer = stringComparer;
      sections       = new Dictionary<string, IniSection>(StringComparer);
    }

    #endregion




    #region Properties & Fields - Public

    public IEqualityComparer<string> StringComparer;

    public bool SaveEmptySections;

    #endregion




    #region Properties Impl - Public

    public int Count
    {
      get { return sections.Count; }
    }

    public ICollection<string> Keys
    {
      get { return sections.Keys; }
    }

    public ICollection<IniSection> Values
    {
      get { return sections.Values; }
    }

    public IniSection this[string section]
    {
      get
      {
        IniSection s;
        if (sections.TryGetValue(section, out s))
          return s;

        s                 = new IniSection(StringComparer);
        sections[section] = s;
        return s;
      }
      set
      {
        var v = value;
        if (v.Comparer != StringComparer)
          v = new IniSection(v, StringComparer);
        sections[section] = v;
      }
    }

    #endregion




    #region Methods Impl

    void ICollection<KeyValuePair<string, IniSection>>.Add(KeyValuePair<string, IniSection> item)
    {
      ((IDictionary<string, IniSection>)sections).Add(item);
    }

    public void Clear()
    {
      sections.Clear();
    }

    bool ICollection<KeyValuePair<string, IniSection>>.Contains(KeyValuePair<string, IniSection> item)
    {
      return ((IDictionary<string, IniSection>)sections).Contains(item);
    }

    void ICollection<KeyValuePair<string, IniSection>>.CopyTo(KeyValuePair<string, IniSection>[] array, int arrayIndex)
    {
      ((IDictionary<string, IniSection>)sections).CopyTo(array, arrayIndex);
    }

    bool ICollection<KeyValuePair<string, IniSection>>.Remove(KeyValuePair<string, IniSection> item)
    {
      return ((IDictionary<string, IniSection>)sections).Remove(item);
    }

    bool IDictionary<string, IniSection>.TryGetValue(string key, out IniSection value)
    {
      return TryGetSection(key, out value);
    }

    public bool Remove(string section)
    {
      return sections.Remove(section);
    }

    void IDictionary<string, IniSection>.Add(string key, IniSection value)
    {
      Add(key, value);
    }

    bool IDictionary<string, IniSection>.ContainsKey(string key)
    {
      return ContainsSection(key);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<KeyValuePair<string, IniSection>> GetEnumerator()
    {
      return sections.GetEnumerator();
    }

    #endregion




    #region Methods

    public void Save(string path, FileMode mode = FileMode.Create)
    {
      using (var stream = new FileStream(path, mode, FileAccess.Write))
        Save(stream);
    }

    public void Save(Stream stream)
    {
      using (var writer = new StreamWriter(stream))
        Save(writer);
    }

    public void Save(StreamWriter writer)
    {
      foreach (var section in sections)
        if (section.Value.Count > 0 || SaveEmptySections)
        {
          writer.WriteLine(string.Format("[{0}]", section.Key.Trim()));
          foreach (var kvp in section.Value)
            writer.WriteLine(string.Format("{0}={1}", kvp.Key, kvp.Value));
          writer.WriteLine("");
        }
    }

    public void Load(string path, bool ordered = false)
    {
      using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
        Load(stream, ordered);
    }

    public void Load(Stream stream, bool ordered = false)
    {
      using (var reader = new StreamReader(stream))
        Load(reader, ordered);
    }

    public void Load(StreamReader reader, bool ordered = false)
    {
      IniSection section = null;

      while (!reader.EndOfStream)
      {
        var line = reader.ReadLine();

        if (line != null)
        {
          var trimStart = line.TrimStart();

          if (trimStart.Length > 0)
          {
            if (trimStart[0] == '[')
            {
              var sectionEnd = trimStart.IndexOf(']');
              if (sectionEnd > 0)
              {
                var sectionName = trimStart.Substring(1, sectionEnd - 1).Trim();
                section               = new IniSection(StringComparer) { Ordered = ordered };
                sections[sectionName] = section;
              }
            }
            else if (section != null && trimStart[0] != ';')
            {
              string   key;
              IniValue val;

              if (LoadValue(line, out key, out val))
                section[key] = val;
            }
          }
        }
      }
    }

    private bool LoadValue(string line, out string key, out IniValue val)
    {
      var assignIndex = line.IndexOf('=');
      if (assignIndex <= 0)
      {
        key = null;
        val = null;
        return false;
      }

      key = line.Substring(0, assignIndex).Trim();
      var value = line.Substring(assignIndex + 1);

      val = new IniValue(value);
      return true;
    }

    public bool ContainsSection(string section)
    {
      return sections.ContainsKey(section);
    }

    public bool TryGetSection(string section, out IniSection result)
    {
      return sections.TryGetValue(section, out result);
    }

    public IniSection Add(string section, Dictionary<string, IniValue> values, bool ordered = false)
    {
      return Add(section, new IniSection(values, StringComparer) { Ordered = ordered });
    }

    public IniSection Add(string section, IniSection value)
    {
      if (value.Comparer != StringComparer)
        value = new IniSection(value, StringComparer);
      sections.Add(section, value);
      return value;
    }

    public IniSection Add(string section, bool ordered = false)
    {
      var value = new IniSection(StringComparer) { Ordered = ordered };
      sections.Add(section, value);
      return value;
    }

    public string GetContents()
    {
      using (var stream = new MemoryStream())
      {
        Save(stream);
        stream.Flush();
        var builder = new StringBuilder(Encoding.UTF8.GetString(stream.ToArray()));
        return builder.ToString();
      }
    }

    #endregion




    private class CaseInsensitiveStringComparer : IEqualityComparer<string>
    {
      public bool Equals(string x, string y)
      {
        return String.Compare(x, y, true) == 0;
      }

      public int GetHashCode(string obj)
      {
        return obj.ToLowerInvariant().GetHashCode();
      }

#if JS
        public new bool Equals(object x, object y) {
            var xs = x as string;
            var ys = y as string;
            if (xs == null || ys == null) {
                return xs == null && ys == null;
            }
            return Equals(xs, ys);
        }

        public int GetHashCode(object obj) {
            if (obj is string) {
                return GetHashCode((string)obj);
            }
            return obj.ToStringInvariant().ToLowerInvariant().GetHashCode();
        }
#endif
    }
  }

  public class IniSection : IEnumerable<KeyValuePair<string, IniValue>>, IDictionary<string, IniValue>
  {
    #region Properties & Fields - Non-Public

    private readonly Dictionary<string, IniValue> values;

    bool ICollection<KeyValuePair<string, IniValue>>.IsReadOnly
    {
      get { return ((IDictionary<string, IniValue>)values).IsReadOnly; }
    }

    #endregion




    #region Constructors

    public IniSection()
      : this(IniFile.DefaultComparer) { }

    public IniSection(IEqualityComparer<string> stringComparer)
    {
      values = new Dictionary<string, IniValue>(stringComparer);
    }

    public IniSection(Dictionary<string, IniValue> values)
      : this(values, IniFile.DefaultComparer) { }

    public IniSection(Dictionary<string, IniValue> values, IEqualityComparer<string> stringComparer)
    {
      this.values = new Dictionary<string, IniValue>(values, stringComparer);
    }

    public IniSection(IniSection values)
      : this(values, IniFile.DefaultComparer) { }

    public IniSection(IniSection values, IEqualityComparer<string> stringComparer)
    {
      this.values = new Dictionary<string, IniValue>(values.values, stringComparer);
    }

    #endregion




    #region Properties & Fields - Public

    public IEqualityComparer<string> Comparer
    {
      get { return values.Comparer; }
    }

    #endregion




    #region Properties Impl - Public

    public int Count
    {
      get { return values.Count; }
    }

    /// <summary>
    ///   Returns this IniSection's collection of keys. If the IniSection is ordered, the keys
    ///   will be returned in order.
    /// </summary>
    public ICollection<string> Keys
    {
      get { return Ordered ? (ICollection<string>)orderedKeys : values.Keys; }
    }

    /// <summary>
    ///   Returns the values in this IniSection. These values are always out of order. To get
    ///   ordered values from an IniSection call GetOrderedValues instead.
    /// </summary>
    public ICollection<IniValue> Values
    {
      get { return values.Values; }
    }

    public IniValue this[string name]
    {
      get
      {
        IniValue val;
        if (values.TryGetValue(name, out val))
          return val;

        return IniValue.Default;
      }
      set
      {
        if (Ordered && !orderedKeys.Contains(name, Comparer))
          orderedKeys.Add(name);
        values[name] = value;
      }
    }

    #endregion




    #region Methods Impl

    void ICollection<KeyValuePair<string, IniValue>>.Add(KeyValuePair<string, IniValue> item)
    {
      ((IDictionary<string, IniValue>)values).Add(item);
      if (Ordered)
        orderedKeys.Add(item.Key);
    }

    public void Clear()
    {
      values.Clear();
      if (Ordered)
        orderedKeys.Clear();
    }

    bool ICollection<KeyValuePair<string, IniValue>>.Contains(KeyValuePair<string, IniValue> item)
    {
      return ((IDictionary<string, IniValue>)values).Contains(item);
    }

    void ICollection<KeyValuePair<string, IniValue>>.CopyTo(KeyValuePair<string, IniValue>[] array, int arrayIndex)
    {
      ((IDictionary<string, IniValue>)values).CopyTo(array, arrayIndex);
    }

    bool ICollection<KeyValuePair<string, IniValue>>.Remove(KeyValuePair<string, IniValue> item)
    {
      var ret = ((IDictionary<string, IniValue>)values).Remove(item);
      if (Ordered && ret)
        for (int i = 0; i < orderedKeys.Count; i++)
          if (Comparer.Equals(orderedKeys[i], item.Key))
          {
            orderedKeys.RemoveAt(i);
            break;
          }

      return ret;
    }

    public void Add(string key, IniValue value)
    {
      values.Add(key, value);
      if (Ordered)
        orderedKeys.Add(key);
    }

    public bool ContainsKey(string key)
    {
      return values.ContainsKey(key);
    }

    public bool Remove(string key)
    {
      var ret = values.Remove(key);
      if (Ordered && ret)
        for (int i = 0; i < orderedKeys.Count; i++)
          if (Comparer.Equals(orderedKeys[i], key))
          {
            orderedKeys.RemoveAt(i);
            break;
          }

      return ret;
    }

    public bool TryGetValue(string key, out IniValue value)
    {
      return values.TryGetValue(key, out value);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<KeyValuePair<string, IniValue>> GetEnumerator()
    {
      if (Ordered)
        return GetOrderedEnumerator();
      else
        return values.GetEnumerator();
    }

    #endregion




    #region Methods

    private IEnumerator<KeyValuePair<string, IniValue>> GetOrderedEnumerator()
    {
      for (int i = 0; i < orderedKeys.Count; i++)
        yield return new KeyValuePair<string, IniValue>(orderedKeys[i], values[orderedKeys[i]]);
    }

    public static implicit operator IniSection(Dictionary<string, IniValue> dict)
    {
      return new IniSection(dict);
    }

    public static explicit operator Dictionary<string, IniValue>(IniSection section)
    {
      return section.values;
    }

    #endregion




    #region Ordered

    private List<string> orderedKeys;

    public int IndexOf(string key)
    {
      if (!Ordered)
        throw new InvalidOperationException("Cannot call IndexOf(string) on IniSection: section was not ordered.");

      return IndexOf(key, 0, orderedKeys.Count);
    }

    public int IndexOf(string key, int index)
    {
      if (!Ordered)
        throw new InvalidOperationException("Cannot call IndexOf(string, int) on IniSection: section was not ordered.");

      return IndexOf(key, index, orderedKeys.Count - index);
    }

    public int IndexOf(string key, int index, int count)
    {
      if (!Ordered)
        throw new InvalidOperationException("Cannot call IndexOf(string, int, int) on IniSection: section was not ordered.");

      if (index < 0 || index > orderedKeys.Count)
        throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");

      if (count < 0)
        throw new IndexOutOfRangeException("Count cannot be less than zero." + Environment.NewLine + "Parameter name: count");

      if (index + count > orderedKeys.Count)
        throw new ArgumentException(
          "Index and count were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");

      var end = index + count;
      for (int i = index; i < end; i++)
        if (Comparer.Equals(orderedKeys[i], key))
          return i;

      return -1;
    }

    public int LastIndexOf(string key)
    {
      if (!Ordered)
        throw new InvalidOperationException("Cannot call LastIndexOf(string) on IniSection: section was not ordered.");

      return LastIndexOf(key, 0, orderedKeys.Count);
    }

    public int LastIndexOf(string key, int index)
    {
      if (!Ordered)
        throw new InvalidOperationException("Cannot call LastIndexOf(string, int) on IniSection: section was not ordered.");

      return LastIndexOf(key, index, orderedKeys.Count - index);
    }

    public int LastIndexOf(string key, int index, int count)
    {
      if (!Ordered)
        throw new InvalidOperationException("Cannot call LastIndexOf(string, int, int) on IniSection: section was not ordered.");

      if (index < 0 || index > orderedKeys.Count)
        throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");

      if (count < 0)
        throw new IndexOutOfRangeException("Count cannot be less than zero." + Environment.NewLine + "Parameter name: count");

      if (index + count > orderedKeys.Count)
        throw new ArgumentException(
          "Index and count were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");

      var end = index + count;
      for (int i = end - 1; i >= index; i--)
        if (Comparer.Equals(orderedKeys[i], key))
          return i;

      return -1;
    }

    public void Insert(int index, string key, IniValue value)
    {
      if (!Ordered)
        throw new InvalidOperationException("Cannot call Insert(int, string, IniValue) on IniSection: section was not ordered.");

      if (index < 0 || index > orderedKeys.Count)
        throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");

      values.Add(key, value);
      orderedKeys.Insert(index, key);
    }

    public void InsertRange(int index, IEnumerable<KeyValuePair<string, IniValue>> collection)
    {
      if (!Ordered)
        throw new InvalidOperationException(
          "Cannot call InsertRange(int, IEnumerable<KeyValuePair<string, IniValue>>) on IniSection: section was not ordered.");

      if (collection == null)
        throw new ArgumentNullException("Value cannot be null." + Environment.NewLine + "Parameter name: collection");

      if (index < 0 || index > orderedKeys.Count)
        throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");

      foreach (var kvp in collection)
      {
        Insert(index, kvp.Key, kvp.Value);
        index++;
      }
    }

    public void RemoveAt(int index)
    {
      if (!Ordered)
        throw new InvalidOperationException("Cannot call RemoveAt(int) on IniSection: section was not ordered.");

      if (index < 0 || index > orderedKeys.Count)
        throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");

      var key = orderedKeys[index];
      orderedKeys.RemoveAt(index);
      values.Remove(key);
    }

    public void RemoveRange(int index, int count)
    {
      if (!Ordered)
        throw new InvalidOperationException("Cannot call RemoveRange(int, int) on IniSection: section was not ordered.");

      if (index < 0 || index > orderedKeys.Count)
        throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");

      if (count < 0)
        throw new IndexOutOfRangeException("Count cannot be less than zero." + Environment.NewLine + "Parameter name: count");

      if (index + count > orderedKeys.Count)
        throw new ArgumentException(
          "Index and count were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");

      for (int i = 0; i < count; i++)
        RemoveAt(index);
    }

    public void Reverse()
    {
      if (!Ordered)
        throw new InvalidOperationException("Cannot call Reverse() on IniSection: section was not ordered.");

      orderedKeys.Reverse();
    }

    public void Reverse(int index, int count)
    {
      if (!Ordered)
        throw new InvalidOperationException("Cannot call Reverse(int, int) on IniSection: section was not ordered.");

      if (index < 0 || index > orderedKeys.Count)
        throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");

      if (count < 0)
        throw new IndexOutOfRangeException("Count cannot be less than zero." + Environment.NewLine + "Parameter name: count");

      if (index + count > orderedKeys.Count)
        throw new ArgumentException(
          "Index and count were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");

      orderedKeys.Reverse(index, count);
    }

    public ICollection<IniValue> GetOrderedValues()
    {
      if (!Ordered)
        throw new InvalidOperationException("Cannot call GetOrderedValues() on IniSection: section was not ordered.");

      var list = new List<IniValue>();
      for (int i = 0; i < orderedKeys.Count; i++)
        list.Add(values[orderedKeys[i]]);
      return list;
    }

    public IniValue this[int index]
    {
      get
      {
        if (!Ordered)
          throw new InvalidOperationException("Cannot index IniSection using integer key: section was not ordered.");

        if (index < 0 || index >= orderedKeys.Count)
          throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");

        return values[orderedKeys[index]];
      }
      set
      {
        if (!Ordered)
          throw new InvalidOperationException("Cannot index IniSection using integer key: section was not ordered.");

        if (index < 0 || index >= orderedKeys.Count)
          throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");

        var key = orderedKeys[index];
        values[key] = value;
      }
    }

    public bool Ordered
    {
      get { return orderedKeys != null; }
      set
      {
        if (Ordered != value)
          orderedKeys = value ? new List<string>(values.Keys) : null;
      }
    }

    #endregion
  }
}
