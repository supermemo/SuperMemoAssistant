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
// Created On:   2019/01/20 08:10
// Modified On:  2019/01/26 01:16
// Modified By:  Alexis

#endregion




using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SuperMemoAssistant.Sys.IO
{
  /// <summary>An ordered collection of unique <see cref="NormalizedPath" />.</summary>
  /// <typeparam name="TPath">The type of the path (file or directory).</typeparam>
  /// https://github.com/Wyamio/Wyam/ Copyright (c) 2014 Dave Glick
  public class PathCollection<TPath> : IReadOnlyList<TPath>
    where TPath : NormalizedPath
  {
    #region Properties & Fields - Non-Public

    private readonly List<TPath> _paths;

    #endregion




    #region Constructors

    /// <summary>Initializes a new path collection.</summary>
    public PathCollection()
      : this(Enumerable.Empty<TPath>()) { }

    /// <summary>Initializes a new path collection.</summary>
    /// <param name="paths">The paths.</param>
    public PathCollection(IEnumerable<TPath> paths)
    {
      _paths = new List<TPath>(paths.Distinct<TPath>(new PathEqualityComparer()));
    }

    #endregion




    #region Properties Impl - Public

    /// <summary>Gets the number of directories in the collection.</summary>
    /// <value>The number of directories in the collection.</value>
    public int Count => _paths.Count;

    /// <summary>Gets or sets the <see cref="DirectoryPath" /> at the specified index.</summary>
    /// <value>The <see cref="DirectoryPath" /> at the specified index.</value>
    /// <param name="index">The index.</param>
    /// <returns>The path at the specified index.</returns>
    public TPath this[int index] { get { return _paths[index]; } set { _paths[index] = value; } }

    #endregion




    #region Methods Impl

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>Returns an enumerator that iterates through the collection.</summary>
    /// <returns>
    ///   An <c>IEnumerator&lt;TPath&gt;</c> that can be used to iterate through the
    ///   collection.
    /// </returns>
    public IEnumerator<TPath> GetEnumerator()
    {
      return _paths.GetEnumerator();
    }

    #endregion




    #region Methods

    /// <summary>Adds the specified path to the collection.</summary>
    /// <param name="path">The path to add.</param>
    /// <returns><c>true</c> if the path was added; <c>false</c> if the path was already present.</returns>
    public bool Add(TPath path)
    {
      if (_paths.Contains(path, new PathEqualityComparer()))
        return false;

      _paths.Add(path);
      return true;
    }

    /// <summary>Adds the specified paths to the collection.</summary>
    /// <param name="paths">The paths to add.</param>
    public void AddRange(IEnumerable<TPath> paths)
    {
      if (paths == null)
        throw new ArgumentNullException(nameof(paths));

      foreach (TPath path in paths)
        _paths.Add(path);
    }

    /// <summary>Clears all paths from the collection.</summary>
    public void Clear()
    {
      _paths.Clear();
    }

    /// <summary>Determines whether the collection contains the specified path.</summary>
    /// <param name="path">The path.</param>
    /// <returns><c>true</c> if the collection contains the path, otherwise <c>false</c>.</returns>
    public bool Contains(TPath path)
    {
      return _paths.Contains(path, new PathEqualityComparer());
    }

    /// <summary>Removes the specified path.</summary>
    /// <param name="path">The path to remove.</param>
    /// <returns><c>true</c> if the collection contained the path, otherwise <c>false</c>.</returns>
    public bool Remove(TPath path)
    {
      int index = IndexOf(path);
      if (index == -1)
        return false;

      _paths.RemoveAt(index);
      return true;
    }

    /// <summary>Removes the specified paths from the collection.</summary>
    /// <param name="paths">The paths to remove.</param>
    public void RemoveRange(IEnumerable<TPath> paths)
    {
      if (paths == null)
        throw new ArgumentNullException(nameof(paths));

      foreach (TPath path in paths)
        _paths.Remove(path);
    }

    /// <summary>Returns the index of the specified path.</summary>
    /// <param name="path">The path.</param>
    /// <returns>The index of the specified path, or -1 if not found.</returns>
    public int IndexOf(TPath path)
    {
      return _paths.FindIndex(x => x.Equals(path));
    }

    /// <summary>Inserts the path at the specified index.</summary>
    /// <param name="index">The index where the path should be inserted.</param>
    /// <param name="path">The path to insert.</param>
    /// <returns>
    ///   <c>true</c> if the collection did not contain the path and it was inserted, otherwise
    ///   <c>false</c>
    /// </returns>
    public bool Insert(int   index,
                       TPath path)
    {
      if (_paths.Contains(path, new PathEqualityComparer()))
        return false;

      _paths.Insert(index, path);
      return true;
    }

    /// <summary>Removes the path at the specified index.</summary>
    /// <param name="index">The index where the path should be removed.</param>
    public void RemoveAt(int index)
    {
      _paths.RemoveAt(index);
    }

    #endregion
  }
}
