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
// Created On:   2019/04/22 16:16
// Modified On:  2019/04/29 12:32
// Modified By:  Alexis

#endregion




using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Forge.Forms;
using PropertyChanged;
using SuperMemoAssistant.Sys.Windows.Input;

namespace SuperMemoAssistant.Services.UI.Forms.Types
{
  public class CrudList<T> : CrudList, IList<T>, ICollection<T>, IEnumerable<T>, IReadOnlyList<T>, IReadOnlyCollection<T>
    where T : new()
  {
    #region Constructors

    /// <inheritdoc />
    public CrudList(string title, ObservableCollection<T> backingCollection)
      : base(title)
    {
      BackingCollection = backingCollection;
      CollectionView = CollectionViewSource.GetDefaultView(this);
    }

    #endregion




    #region Properties & Fields - Public

    public ObservableCollection<T> BackingCollection { get; set; }

    public Func<CrudList<T>, Task>    NewFunc    { get; set; }
    public Func<CrudList<T>, T, Task> EditFunc   { get; set; }
    public Func<CrudList<T>, T, Task> DeleteFunc { get; set; }

    #endregion




    #region Properties Impl - Public

    public override ICommand NewCommand    => new AsyncRelayCommand(NewItem);
    public override ICommand DeleteCommand => new AsyncRelayCommand<T>(DeleteItem);
    public override ICommand EditCommand   => new AsyncRelayCommand<T>(EditItem);

    public int Count => BackingCollection.Count;

    /// <inheritdoc />
    public bool IsReadOnly => ((ICollection<T>)BackingCollection).IsReadOnly;

    public T this[int index] { get => BackingCollection[index]; set => BackingCollection[index] = value; }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public void Add(T item)
    {
      BackingCollection.Add(item);
    }

    /// <inheritdoc />
    public void Clear()
    {
      BackingCollection.Clear();
    }

    /// <inheritdoc />
    public bool Contains(T item)
    {
      return BackingCollection.Contains(item);
    }

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex)
    {
      BackingCollection.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    public bool Remove(T item)
    {
      return BackingCollection.Remove(item);
    }

    /// <inheritdoc />
    public override IEnumerator GetEnumerator()
    {
      return BackingCollection.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return BackingCollection.GetEnumerator();
    }

    /// <inheritdoc />
    public int IndexOf(T item)
    {
      return BackingCollection.IndexOf(item);
    }

    /// <inheritdoc />
    public void Insert(int index, T item)
    {
      BackingCollection.Insert(index, item);
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
      BackingCollection.RemoveAt(index);
    }

    #endregion




    #region Methods

    protected virtual async Task NewItem()
    {
      if (NewFunc != null)
      {
        await NewFunc(this);
        return;
      }

      var item = new T();
      var res  = await Show.Window().For<T>(item);

      if (res.Action is "cancel")
        return;

      Application.Current.Dispatcher.Invoke(() => Add(item));
    }

    protected virtual Task DeleteItem(T item)
    {
      if (DeleteFunc != null)
        return DeleteFunc(this, item);

      if (Show.Window().For(new Confirmation("Are you sure ?")).Result.Model.Confirmed)
        Application.Current.Dispatcher.Invoke(() => Remove(item));

      return Task.CompletedTask;
    }

    protected virtual Task EditItem(T item)
    {
      if (EditFunc != null)
        return EditFunc(this, item);

      return Show.Window().For<T>(item);
    }

    public static implicit operator ObservableCollection<T>(CrudList<T> self)
    {
      return self.BackingCollection;
    }

    #endregion




    #region Events

    /// <inheritdoc />
    public override event NotifyCollectionChangedEventHandler CollectionChanged
    {
      add => BackingCollection.CollectionChanged += value;
      remove => BackingCollection.CollectionChanged -= value;
    }


    public override event PropertyChangedEventHandler PropertyChanged
    {
      add => ((INotifyPropertyChanged)BackingCollection).PropertyChanged += value;
      remove => ((INotifyPropertyChanged)BackingCollection).PropertyChanged -= value;
    }

    #endregion
  }

  [DoNotNotify]
  public abstract class CrudList : IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
  {
    #region Properties & Fields - Non-Public

    private string            _sortingPropertyName;
    private ListSortDirection _sortingDirection = ListSortDirection.Ascending;

    #endregion




    #region Constructors

    protected CrudList(string title)
    {
      Title          = title;
    }

    #endregion




    #region Properties & Fields - Public

    public string     Title  { get; set; }
    public GridLength Height { get; set; } = new GridLength(1, GridUnitType.Star);
    public string SortingPropertyName
    {
      get => _sortingPropertyName;
      set
      {
        _sortingPropertyName = value;
        UpdateSortDescriptions();
      }
    }
    public ListSortDirection SortingDirection
    {
      get => _sortingDirection;
      set
      {
        _sortingDirection = value;
        UpdateSortDescriptions();
      }
    }


    public ICollectionView CollectionView { get; protected set; }

    #endregion




    #region Methods

    private void UpdateSortDescriptions()
    {
      CollectionView.SortDescriptions.Clear();
      CollectionView.SortDescriptions.Add(new SortDescription(SortingPropertyName, SortingDirection));
      CollectionView.Refresh();
    }

    #endregion




    #region Methods Abs

    public abstract ICommand NewCommand    { get; }
    public abstract ICommand DeleteCommand { get; }
    public abstract ICommand EditCommand   { get; }

    /// <inheritdoc />
    public abstract IEnumerator GetEnumerator();

    #endregion




    #region Events

    /// <inheritdoc />
    public abstract event NotifyCollectionChangedEventHandler CollectionChanged;
    /// <inheritdoc />
    public abstract event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
