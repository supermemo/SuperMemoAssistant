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
// Created On:   2020/01/13 16:38
// Modified On:  2020/01/22 16:29
// Modified By:  Alexis

#endregion




using System;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace SuperMemoAssistant.Interop.SuperMemo.Core
{
  /// <summary>
  /// Represents a SuperMemo collection on disk
  /// </summary>
  [Serializable]
  public class SMCollection : IEquatable<SMCollection>
  {
    #region Constructors

    /// <summary>
    /// Instantiates a new SM collection
    /// </summary>
    public SMCollection() { }

    /// <summary>
    /// Instantiates a new SM collection. Name is inferred from the .kno file name
    /// </summary>
    /// <param name="knoFilePath">File path to a .kno SuperMemo collection file</param>
    /// <param name="lastOpen">The last time that collection was open (usually DateTime.Now for a newly imported collection)</param>
    public SMCollection(string   knoFilePath,
                        DateTime lastOpen)
    {
      Name     = System.IO.Path.GetFileNameWithoutExtension(knoFilePath);
      Path     = System.IO.Path.GetDirectoryName(knoFilePath);
      LastOpen = lastOpen;
    }
    
    /// <summary>
    /// Instantiates a new SM collection
    /// </summary>
    /// <param name="name">The .kno file name (without the .kno extension)</param>
    /// <param name="dirPath">Directory path to where a <paramref name="name"/> .kno SuperMemo collection file is located</param>
    /// <param name="lastOpen">The last time that collection was open (usually DateTime.Now for a newly imported collection)</param>
    public SMCollection(string   name,
                        string   dirPath,
                        DateTime lastOpen)
    {
      Name     = name;
      Path     = dirPath;
      LastOpen = lastOpen;
    }

    #endregion




    #region Properties & Fields - Public

    /// <summary>
    /// The collection's name (equals the .kno filename without extension)
    /// </summary>
    public string   Name     { get; set; }

    /// <summary>
    /// The collection's path
    /// </summary>
    public string   Path     { get; set; }

    /// <summary>
    /// When was the last time this collection was open in SMA
    /// </summary>
    public DateTime LastOpen { get; set; }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType() != GetType())
        return false;

      return Equals((SMCollection)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      unchecked
      {
        return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Path != null ? Path.GetHashCode() : 0);
      }
    }

    /// <inheritdoc />
    public bool Equals(SMCollection other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;

      return string.Equals(Name, other.Name) && string.Equals(Path, other.Path);
    }

    #endregion




    #region Methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(SMCollection left,
                                   SMCollection right)
    {
      return Equals(left, right);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(SMCollection left,
                                   SMCollection right)
    {
      return !Equals(left, right);
    }

    #endregion
  }
}
