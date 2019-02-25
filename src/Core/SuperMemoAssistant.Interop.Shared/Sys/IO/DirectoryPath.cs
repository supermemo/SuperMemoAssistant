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
// Modified On:  2019/01/26 01:36
// Modified By:  Alexis

#endregion




using System;
using System.IO;
using System.Linq;

namespace SuperMemoAssistant.Sys.IO
{
  /// <summary>Represents a directory path.</summary>
  /// https://github.com/Wyamio/Wyam/ Copyright (c) 2014 Dave Glick
  public sealed class DirectoryPath : NormalizedPath
  {
    #region Constructors

    // Initially based on code from Cake (http://cakebuild.net/)

    /// <summary>
    ///   Initializes a new instance of the <see cref="DirectoryPath" /> class. The path will
    ///   be considered absolute if the underlying OS file system considers it absolute.
    /// </summary>
    /// <param name="path">The path.</param>
    public DirectoryPath(string path)
      : base(path, PathKind.RelativeOrAbsolute) { }

    /// <summary>Initializes a new instance of the <see cref="DirectoryPath" /> class.</summary>
    /// <param name="path">The path.</param>
    /// <param name="pathKind">Specifies whether the path is relative, absolute, or indeterminate.</param>
    public DirectoryPath(string   path,
                         PathKind pathKind)
      : base(path, pathKind) { }

    /// <summary>
    ///   Initializes a new instance of the <see cref="DirectoryPath" /> class with the
    ///   specified file provider. The path will be considered absolute if the underlying OS file
    ///   system considers it absolute.
    /// </summary>
    /// <param name="fileProvider">The file provider.</param>
    /// <param name="path">The path.</param>
    public DirectoryPath(string fileProvider,
                         string path)
      : base(fileProvider, path, PathKind.RelativeOrAbsolute) { }

    /// <summary>
    ///   Initializes a new instance of the <see cref="DirectoryPath" /> class with the
    ///   specified file provider.
    /// </summary>
    /// <param name="fileProvider">The file provider.</param>
    /// <param name="path">The path.</param>
    /// <param name="pathKind">Specifies whether the path is relative, absolute, or indeterminate.</param>
    public DirectoryPath(string   fileProvider,
                         string   path,
                         PathKind pathKind)
      : base(fileProvider, path, pathKind) { }

    /// <summary>
    ///   Initializes a new instance of the <see cref="DirectoryPath" /> class with the
    ///   specified file provider. The path will be considered absolute if the underlying OS file
    ///   system considers it absolute.
    /// </summary>
    /// <param name="fileProvider">The file provider.</param>
    /// <param name="path">The path.</param>
    public DirectoryPath(Uri    fileProvider,
                         string path)
      : base(fileProvider, path, PathKind.RelativeOrAbsolute) { }

    /// <summary>
    ///   Initializes a new instance of the <see cref="DirectoryPath" /> class with the
    ///   specified file provider.
    /// </summary>
    /// <param name="fileProvider">The file provider.</param>
    /// <param name="path">The path.</param>
    /// <param name="pathKind">Specifies whether the path is relative, absolute, or indeterminate.</param>
    public DirectoryPath(Uri      fileProvider,
                         string   path,
                         PathKind pathKind)
      : base(fileProvider, path, pathKind) { }

    /// <summary>
    ///   Initializes a new instance of the <see cref="DirectoryPath" /> class with the
    ///   specified file provider and/or path.
    /// </summary>
    /// <param name="path">The path (and file provider if this is an absolute URI).</param>
    public DirectoryPath(Uri path)
      : base(path) { }

    #endregion




    #region Properties & Fields - Public

    /// <summary>Gets the name of the directory.</summary>
    /// <value>The directory name.</value>
    /// <remarks>
    ///   If this is passed a file path, it will return the file name. This is by-and-large
    ///   equivalent to how DirectoryInfo handles this scenario. If we wanted to return the *actual*
    ///   directory name, we'd need to pull in IFileSystem, and do various checks to make sure things
    ///   exists.
    /// </remarks>
    public string Name => Segments.Length == 0 ? FullPath : Segments.Last();

    /// <summary>Gets the parent path or <c>null</c> if this is a root path.</summary>
    /// <value>The parent path or <c>null</c> if this is a root path.</value>
    public DirectoryPath Parent
    {
      get
      {
        string directory = Path.GetDirectoryName(FullPath);
        if (string.IsNullOrWhiteSpace(directory))
          return null;

        return new DirectoryPath(FileProvider, directory);
      }
    }

    /// <summary>
    ///   Gets current path relative to it's root. If this is already a relative path or there
    ///   is no root path, this just returns the current path.
    /// </summary>
    /// <value>The current path relative to it's root.</value>
    public DirectoryPath RootRelative
    {
      get
      {
        if (!IsAbsolute)
          return this;

        DirectoryPath root = Root;
        return root.FullPath == "."
          ? this
          : new DirectoryPath(FullPath.Substring(root.FullPath.Length), PathKind.Relative);
      }
    }

    #endregion




    #region Methods

    /// <summary>
    ///   Combines the current path with the file name of a <see cref="FilePath" />. The
    ///   current file provider is maintained.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    ///   A combination of the current path and the file name of the provided
    ///   <see cref="FilePath" />.
    /// </returns>
    public FilePath GetFilePath(FilePath path)
    {
      if (path == null)
        throw new ArgumentNullException(nameof(path));

      return new FilePath(FileProvider, Path.Combine(FullPath, path.FileName));
    }

    /// <summary>
    ///   Get the relative path to another directory. If this path and the target path do not
    ///   share the same file provider, the target path is returned.
    /// </summary>
    /// <param name="target">The target directory path.</param>
    /// <returns>A <see cref="DirectoryPath" />.</returns>
    public DirectoryPath GetRelativePath(DirectoryPath target)
    {
      return RelativePathResolver.Resolve(this, target);
    }

    /// <summary>
    ///   Get the relative path to another file. If this path and the target path do not share
    ///   the same file provider, the target path is returned.
    /// </summary>
    /// <param name="target">The target file path.</param>
    /// <returns>A <see cref="FilePath" />.</returns>
    public FilePath GetRelativePath(FilePath target)
    {
      return RelativePathResolver.Resolve(this, target);
    }

    /// <summary>
    ///   Combines the current path with a <see cref="FilePath" />. If the provided
    ///   <see cref="FilePath" /> is not relative, then it is returned.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    ///   A combination of the current path and the provided <see cref="FilePath" />, unless
    ///   the provided <see cref="FilePath" /> is absolute in which case it is returned.
    /// </returns>
    public FilePath CombineFile(FilePath path)
    {
      if (path == null)
        throw new ArgumentNullException(nameof(path));

      return !path.IsRelative ? path : new FilePath(FileProvider, Path.Combine(FullPath, path.FullPath));
    }

    /// <summary>
    ///   Combines the current path with another <see cref="DirectoryPath" />. If the provided
    ///   <see cref="DirectoryPath" /> is not relative, then it is returned.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    ///   A combination of the current path and the provided <see cref="DirectoryPath" />,
    ///   unless the provided <see cref="DirectoryPath" /> is absolute in which case it is returned.
    /// </returns>
    public DirectoryPath Combine(DirectoryPath path)
    {
      if (path == null)
        throw new ArgumentNullException(nameof(path));

      return !path.IsRelative ? path : new DirectoryPath(FileProvider, Path.Combine(FullPath, path.FullPath));
    }

    /// <summary>Determines whether the given path refers to an existing directory on disk.</summary>
    /// <returns>
    ///   <see langword="true" /> if the path refers to an existing directory;
    ///   <see langword="false" /> if the directory does not exist or an error occurs when trying to
    ///   determine if the specified directory exists.
    /// </returns>
    public bool Exists() => Directory.Exists(FullPath);

    /// <summary>
    ///   Deletes the specified directory and, if indicated, any subdirectories and files in
    ///   the directory.
    /// </summary>
    /// <param name="recursive">
    ///   <see langword="true" /> to remove directories, subdirectories, and
    ///   files in directory; otherwise, <see langword="false" />.
    /// </param>
    /// <exception cref="T:System.IO.IOException">
    ///   A file with the same name and location specified by
    ///   directory exists.-or-The directory specified by directory is read-only, or
    ///   <paramref name="recursive" /> is <see langword="false" /> and directory is not an empty
    ///   directory. -or-The directory is the application's current working directory. -or-The
    ///   directory contains a read-only file.-or-The directory is being used by another process.
    /// </exception>
    /// <exception cref="T:System.UnauthorizedAccessException">
    ///   The caller does not have the required
    ///   permission.
    /// </exception>
    /// <exception cref="T:System.ArgumentException">
    ///   directory is a zero-length string, contains only
    ///   white space, or contains one or more invalid characters. You can query for invalid
    ///   characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.
    /// </exception>
    /// <exception cref="T:System.ArgumentNullException">directory is <see langword="null" />. </exception>
    /// <exception cref="T:System.IO.PathTooLongException">
    ///   The specified path, file name, or both
    ///   exceed the system-defined maximum length. For example, on Windows-based platforms, paths
    ///   must be less than 248 characters and file names must be less than 260 characters.
    /// </exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">
    ///   directory does not exist or could not
    ///   be found.-or-The specified path is invalid (for example, it is on an unmapped drive).
    /// </exception>
    public void Delete(bool recursive = true) => Directory.Delete(FullPath, recursive);

    /// <summary>
    ///   Creates all directories and subdirectories in the specified path unless they already
    ///   exist.
    /// </summary>
    /// <returns>
    ///   An object that represents the directory at the specified path. This object is
    ///   returned regardless of whether a directory at the specified path already exists.
    /// </returns>
    /// <exception cref="T:System.IO.IOException">
    ///   The directory specified by directory is a
    ///   file.-or-The network name is not known.
    /// </exception>
    /// <exception cref="T:System.UnauthorizedAccessException">
    ///   The caller does not have the required
    ///   permission.
    /// </exception>
    /// <exception cref="T:System.ArgumentException">
    ///   directory is a zero-length string, contains only
    ///   white space, or contains one or more invalid characters. You can query for invalid
    ///   characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.-or-
    ///   directory is prefixed with, or contains, only a colon character (:).
    /// </exception>
    /// <exception cref="T:System.ArgumentNullException">directory is <see langword="null" />. </exception>
    /// <exception cref="T:System.IO.PathTooLongException">
    ///   The specified path, file name, or both
    ///   exceed the system-defined maximum length. For example, on Windows-based platforms, paths
    ///   must be less than 248 characters and file names must be less than 260 characters.
    /// </exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">
    ///   The specified path is invalid (for
    ///   example, it is on an unmapped drive).
    /// </exception>
    /// <exception cref="T:System.NotSupportedException">
    ///   directory contains a colon character (:) that
    ///   is not part of a drive label ("C:\").
    /// </exception>
    public void Create() => Directory.CreateDirectory(FullPath);

    public void CopyTo(DirectoryPath destinationDir,
                       bool          overwrite = true,
                       DirectoryPath srcDir    = null)
    {
      srcDir = srcDir ?? this;
      var srcDirPath  = srcDir.FullPath;
      var destDirPath = destinationDir.FullPath;

      foreach (var itSrcDirPath in Directory.GetDirectories(srcDirPath))
      {
        string itDirName = Path.GetFileName(itSrcDirPath);

        if (string.IsNullOrWhiteSpace(itDirName))
          continue;

        string itDestDirPath = Path.Combine(destDirPath, itDirName);

        if (Directory.Exists(itDestDirPath) == false)
          Directory.CreateDirectory(itDestDirPath);

        CopyTo(itDestDirPath, overwrite, itSrcDirPath);
      }

      foreach (var file in Directory.GetFiles(srcDirPath))
        File.Copy(file, Path.Combine(destDirPath, Path.GetFileName(file)), overwrite);
    }

    /// <summary>Collapses a <see cref="DirectoryPath" /> containing ellipses.</summary>
    /// <returns>A collapsed <see cref="DirectoryPath" />.</returns>
    public DirectoryPath Collapse() => new DirectoryPath(FileProvider, Collapse(this));

    /// <summary>
    ///   Performs an implicit conversion from <see cref="string" /> to
    ///   <see cref="DirectoryPath" />.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>A <see cref="DirectoryPath" />.</returns>
    public static implicit operator DirectoryPath(string path) => FromString(path);

    /// <summary>Performs a conversion from <see cref="string" /> to <see cref="DirectoryPath" />.</summary>
    /// <param name="path">The path.</param>
    /// <returns>A <see cref="DirectoryPath" />.</returns>
    public static DirectoryPath FromString(string path) =>
      path == null ? null : new DirectoryPath(path);

    /// <summary>
    ///   Performs an implicit conversion from <see cref="Uri" /> to
    ///   <see cref="DirectoryPath" />.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>A <see cref="DirectoryPath" />.</returns>
    public static implicit operator DirectoryPath(Uri path) => FromUri(path);

    /// <summary>Performs a conversion from <see cref="Uri" /> to <see cref="DirectoryPath" />.</summary>
    /// <param name="path">The path.</param>
    /// <returns>A <see cref="DirectoryPath" />.</returns>
    public static DirectoryPath FromUri(Uri path) =>
      path == null ? null : new DirectoryPath(path);

    #endregion
  }
}
