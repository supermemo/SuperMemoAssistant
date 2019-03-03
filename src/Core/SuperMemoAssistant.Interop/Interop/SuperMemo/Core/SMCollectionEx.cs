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
// Created On:   2019/02/13 13:55
// Modified On:  2019/02/22 01:13
// Modified By:  Alexis

#endregion




using System.IO;

namespace SuperMemoAssistant.Interop.SuperMemo.Core
{
  public static class SMCollectionEx
  {
    #region Methods

    public static string GetRootDirPath(this SMCollection collection)
    {
      return Path.Combine(collection.Path, collection.Name);
    }

    public static string GetFilePath(
      this   SMCollection collection,
      params string[]     paths)
    {
      return Path.Combine(collection.Path,
                          collection.Name,
                          Path.Combine(paths));
    }

    public static string GetElementFilePath(
      this SMCollection collection,
      string            filePath)
    {
      return collection.GetFilePath(SMConst.Paths.ElementsFolder,
                                    filePath);
    }

    public static string GetInfoFilePath(
      this SMCollection collection,
      string            fileName)
    {
      return collection.GetFilePath(SMConst.Paths.InfoFolder,
                                    fileName);
    }

    public static string GetRegistryFilePath(
      this SMCollection collection,
      string            fileName)
    {
      return collection.GetFilePath(SMConst.Paths.RegistryFolder,
                                    fileName);
    }

    public static string GetSMAFolder(
      this SMCollection collection)
    {
      return collection.GetFilePath(SMAFileSystem.CollectionSMAFolder);
    }

    public static string GetSMAElementsFolder(
      this SMCollection collection,
      int               elementId = 0)
    {
      return elementId == 0
        ? collection.GetFilePath(SMAFileSystem.CollectionSMAFolder,
                                 SMAFileSystem.CollectionElementsFolder,
                                 elementId.ToString())
        : collection.GetFilePath(SMAFileSystem.CollectionSMAFolder,
                                 SMAFileSystem.CollectionElementsFolder);
    }

    public static string GetSMASystemFolder(
      this SMCollection collection)
    {
      return collection.GetFilePath(SMAFileSystem.CollectionSMAFolder,
                                    SMAFileSystem.CollectionSystemFolder);
    }

    public static string GetSMASystemFilePath(
      this SMCollection collection,
      string            fileName)
    {
      return collection.GetFilePath(SMAFileSystem.CollectionSMAFolder,
                                    SMAFileSystem.CollectionSystemFolder,
                                    fileName);
    }

    public static string GetKnoFilePath(this SMCollection collection)
    {
      return Path.Combine(collection.Path,
                          collection.Name + ".Kno");
    }

    public static string MakeRelative(this SMCollection collection,
                                      string            absolutePath)
    {
      string basePath = collection.GetFilePath();

      return absolutePath.StartsWith(basePath)
        ? absolutePath.Substring(basePath.Length).TrimStart('\\', '/')
        : absolutePath;
    }

    #endregion
  }
}
