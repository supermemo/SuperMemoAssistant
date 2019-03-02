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
// Created On:   2019/01/16 00:50
// Modified On:  2019/01/16 00:51
// Modified By:  Alexis

#endregion




using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

// https://github.com/Athari/Alba.Framework/blob/master/Alba.Framework/Collections/Collections/BiDictionary(TFirst%2CTSecond).cs
namespace SuperMemoAssistant.Sys.Collections
{
  [Serializable]
  [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
  [DebuggerTypeProxy(typeof(DictionaryDebugView<,>))]
  public class ConcurrentBiDictionary<TFirst, TSecond> : IDictionary<TFirst, TSecond>, IReadOnlyDictionary<TFirst, TSecond>, IDictionary
  {
    #region Properties & Fields - Non-Public

    private readonly                 IDictionary<TFirst, TSecond> _firstToSecond = new ConcurrentDictionary<TFirst, TSecond>();
    [NonSerialized] private readonly ReverseDictionary            _reverseDictionary;
    [NonSerialized] private readonly IDictionary<TSecond, TFirst> _secondToFirst = new ConcurrentDictionary<TSecond, TFirst>();

    object ICollection.SyncRoot => ((ICollection)_firstToSecond).SyncRoot;

    bool ICollection.IsSynchronized => ((ICollection)_firstToSecond).IsSynchronized;

    bool IDictionary.IsFixedSize => ((IDictionary)_firstToSecond).IsFixedSize;

    object IDictionary.this[object key]
    {
      get => ((IDictionary)_firstToSecond)[key];
      set
      {
        ((IDictionary)_firstToSecond)[key]   = value;
        ((IDictionary)_secondToFirst)[value] = key;
      }
    }

    ICollection IDictionary.Keys => ((IDictionary)_firstToSecond).Keys;

    IEnumerable<TFirst> IReadOnlyDictionary<TFirst, TSecond>.Keys => ((IReadOnlyDictionary<TFirst, TSecond>)_firstToSecond).Keys;

    ICollection IDictionary.Values => ((IDictionary)_firstToSecond).Values;

    IEnumerable<TSecond> IReadOnlyDictionary<TFirst, TSecond>.Values => ((IReadOnlyDictionary<TFirst, TSecond>)_firstToSecond).Values;

    #endregion




    #region Constructors

    public ConcurrentBiDictionary()
    {
      _reverseDictionary = new ReverseDictionary(this);
    }

    #endregion




    #region Properties & Fields - Public

    public IDictionary<TSecond, TFirst> Reverse => _reverseDictionary;

    #endregion




    #region Properties Impl - Public

    public int Count => _firstToSecond.Count;

    public bool IsReadOnly => _firstToSecond.IsReadOnly || _secondToFirst.IsReadOnly;

    public TSecond this[TFirst key]
    {
      get => _firstToSecond[key];
      set
      {
        _firstToSecond[key]   = value;
        _secondToFirst[value] = key;
      }
    }

    public ICollection<TFirst> Keys => _firstToSecond.Keys;

    public ICollection<TSecond> Values => _firstToSecond.Values;

    #endregion




    #region Methods Impl

    void ICollection.CopyTo(Array array,
                            int   index)
    {
      ((IDictionary)_firstToSecond).CopyTo(array,
                                           index);
    }

    void ICollection<KeyValuePair<TFirst, TSecond>>.Add(KeyValuePair<TFirst, TSecond> item)
    {
      _firstToSecond.Add(item);
      _secondToFirst.Add(item.Reverse());
    }

    bool ICollection<KeyValuePair<TFirst, TSecond>>.Contains(KeyValuePair<TFirst, TSecond> item)
    {
      return _firstToSecond.Contains(item);
    }

    bool ICollection<KeyValuePair<TFirst, TSecond>>.Remove(KeyValuePair<TFirst, TSecond> item)
    {
      return _firstToSecond.Remove(item);
    }

    public void Clear()
    {
      _firstToSecond.Clear();
      _secondToFirst.Clear();
    }

    void ICollection<KeyValuePair<TFirst, TSecond>>.CopyTo(KeyValuePair<TFirst, TSecond>[] array,
                                                           int                             arrayIndex)
    {
      _firstToSecond.CopyTo(array,
                            arrayIndex);
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
      return ((IDictionary)_firstToSecond).GetEnumerator();
    }

    void IDictionary.Add(object key,
                         object value)
    {
      ((IDictionary)_firstToSecond).Add(key,
                                        value);
      ((IDictionary)_secondToFirst).Add(value,
                                        key);
    }

    void IDictionary.Remove(object key)
    {
      var firstToSecond = (IDictionary)_firstToSecond;
      if (!firstToSecond.Contains(key))
        return;

      var value = firstToSecond[key];
      firstToSecond.Remove(key);
      ((IDictionary)_secondToFirst).Remove(value);
    }

    bool IDictionary.Contains(object key)
    {
      return ((IDictionary)_firstToSecond).Contains(key);
    }

    public void Add(TFirst  key,
                    TSecond value)
    {
      _firstToSecond.Add(key,
                         value);
      _secondToFirst.Add(value,
                         key);
    }

    public bool ContainsKey(TFirst key)
    {
      return _firstToSecond.ContainsKey(key);
    }

    public bool TryGetValue(TFirst      key,
                            out TSecond value)
    {
      return _firstToSecond.TryGetValue(key,
                                        out value);
    }

    public bool Remove(TFirst key)
    {
      if (_firstToSecond.TryGetValue(key,
                                     out var value))
      {
        _firstToSecond.Remove(key);
        _secondToFirst.Remove(value);
        return true;
      }
      else
      {
        return false;
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<KeyValuePair<TFirst, TSecond>> GetEnumerator()
    {
      return _firstToSecond.GetEnumerator();
    }

    #endregion




    #region Methods

    [OnDeserialized]
    internal void OnDeserialized(StreamingContext context)
    {
      _secondToFirst.Clear();
      foreach (var item in _firstToSecond)
        _secondToFirst.Add(item.Value,
                           item.Key);
    }

    #endregion




    private class ReverseDictionary : IDictionary<TSecond, TFirst>, IReadOnlyDictionary<TSecond, TFirst>, IDictionary
    {
      #region Properties & Fields - Non-Public

      private readonly ConcurrentBiDictionary<TFirst, TSecond> _owner;

      object ICollection.SyncRoot => ((ICollection)_owner._secondToFirst).SyncRoot;

      bool ICollection.IsSynchronized => ((ICollection)_owner._secondToFirst).IsSynchronized;

      bool IDictionary.IsFixedSize => ((IDictionary)_owner._secondToFirst).IsFixedSize;

      object IDictionary.this[object key]
      {
        get => ((IDictionary)_owner._secondToFirst)[key];
        set
        {
          ((IDictionary)_owner._secondToFirst)[key]   = value;
          ((IDictionary)_owner._firstToSecond)[value] = key;
        }
      }

      ICollection IDictionary.Keys => ((IDictionary)_owner._secondToFirst).Keys;

      IEnumerable<TSecond> IReadOnlyDictionary<TSecond, TFirst>.Keys => ((IReadOnlyDictionary<TSecond, TFirst>)_owner._secondToFirst).Keys;

      ICollection IDictionary.Values => ((IDictionary)_owner._secondToFirst).Values;

      IEnumerable<TFirst> IReadOnlyDictionary<TSecond, TFirst>.Values => ((IReadOnlyDictionary<TSecond, TFirst>)_owner._secondToFirst).Values;

      #endregion




      #region Constructors

      public ReverseDictionary(ConcurrentBiDictionary<TFirst, TSecond> owner)
      {
        _owner = owner;
      }

      #endregion




      #region Properties Impl - Public

      public int Count => _owner._secondToFirst.Count;

      public bool IsReadOnly => _owner._secondToFirst.IsReadOnly || _owner._firstToSecond.IsReadOnly;

      public TFirst this[TSecond key]
      {
        get => _owner._secondToFirst[key];
        set
        {
          _owner._secondToFirst[key]   = value;
          _owner._firstToSecond[value] = key;
        }
      }

      public ICollection<TSecond> Keys => _owner._secondToFirst.Keys;

      public ICollection<TFirst> Values => _owner._secondToFirst.Values;

      #endregion




      #region Methods Impl

      void ICollection.CopyTo(Array array,
                              int   index)
      {
        ((IDictionary)_owner._secondToFirst).CopyTo(array,
                                                    index);
      }

      void ICollection<KeyValuePair<TSecond, TFirst>>.Add(KeyValuePair<TSecond, TFirst> item)
      {
        _owner._secondToFirst.Add(item);
        _owner._firstToSecond.Add(item.Reverse());
      }

      bool ICollection<KeyValuePair<TSecond, TFirst>>.Contains(KeyValuePair<TSecond, TFirst> item)
      {
        return _owner._secondToFirst.Contains(item);
      }

      bool ICollection<KeyValuePair<TSecond, TFirst>>.Remove(KeyValuePair<TSecond, TFirst> item)
      {
        return _owner._secondToFirst.Remove(item);
      }

      public void Clear()
      {
        _owner._secondToFirst.Clear();
        _owner._firstToSecond.Clear();
      }

      void ICollection<KeyValuePair<TSecond, TFirst>>.CopyTo(KeyValuePair<TSecond, TFirst>[] array,
                                                             int                             arrayIndex)
      {
        _owner._secondToFirst.CopyTo(array,
                                     arrayIndex);
      }

      IDictionaryEnumerator IDictionary.GetEnumerator()
      {
        return ((IDictionary)_owner._secondToFirst).GetEnumerator();
      }

      void IDictionary.Add(object key,
                           object value)
      {
        ((IDictionary)_owner._secondToFirst).Add(key,
                                                 value);
        ((IDictionary)_owner._firstToSecond).Add(value,
                                                 key);
      }

      void IDictionary.Remove(object key)
      {
        var firstToSecond = (IDictionary)_owner._secondToFirst;
        if (!firstToSecond.Contains(key))
          return;

        var value = firstToSecond[key];
        firstToSecond.Remove(key);
        ((IDictionary)_owner._firstToSecond).Remove(value);
      }

      bool IDictionary.Contains(object key)
      {
        return ((IDictionary)_owner._secondToFirst).Contains(key);
      }

      public void Add(TSecond key,
                      TFirst  value)
      {
        _owner._secondToFirst.Add(key,
                                  value);
        _owner._firstToSecond.Add(value,
                                  key);
      }

      public bool ContainsKey(TSecond key)
      {
        return _owner._secondToFirst.ContainsKey(key);
      }

      public bool TryGetValue(TSecond    key,
                              out TFirst value)
      {
        return _owner._secondToFirst.TryGetValue(key,
                                                 out value);
      }

      public bool Remove(TSecond key)
      {
        if (_owner._secondToFirst.TryGetValue(key,
                                              out var value))
        {
          _owner._secondToFirst.Remove(key);
          _owner._firstToSecond.Remove(value);
          return true;
        }
        else
        {
          return false;
        }
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      public IEnumerator<KeyValuePair<TSecond, TFirst>> GetEnumerator()
      {
        return _owner._secondToFirst.GetEnumerator();
      }

      #endregion
    }
  }
}
