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
// Modified On:  2020/02/10 10:46
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

    public static string CombinePath(
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
      return collection.CombinePath(SMConst.Paths.ElementsFolder,
                                    filePath);
    }

    public static string GetSMAFolder(
      this SMCollection collection)
    {
      return collection.CombinePath(SMAFileSystem.CollectionSMAFolder);
    }

    public static string GetSMAElementsFolder(
      this SMCollection collection,
      int               elementId = 0)
    {
      return elementId == 0
        ? collection.CombinePath(SMAFileSystem.CollectionSMAFolder,
                                 SMAFileSystem.CollectionElementsFolder,
                                 elementId.ToString())
        : collection.CombinePath(SMAFileSystem.CollectionSMAFolder,
                                 SMAFileSystem.CollectionElementsFolder);
    }

    public static string GetSMAConfigsFolder(
      this SMCollection collection)
    {
      return collection.CombinePath(SMAFileSystem.CollectionSMAFolder,
                                    SMAFileSystem.ConfigsFolder);
    }

    public static string GetSMAConfigsSubFolder(
      this SMCollection collection,
      string            subFolder)
    {
      return collection.CombinePath(SMAFileSystem.CollectionSMAFolder,
                                    SMAFileSystem.ConfigsFolder,
                                    subFolder);
    }

    public static string GetKnoFilePath(this SMCollection collection)
    {
      return Path.Combine(collection.Path,
                          collection.Name + ".Kno");
    }

    public static string MakeRelative(this SMCollection collection,
                                      string            absolutePath)
    {
      string basePath = collection.CombinePath();

      return absolutePath.StartsWith(basePath)
        ? absolutePath.Substring(basePath.Length).TrimStart('\\', '/')
        : absolutePath;
    }

    #endregion
  }
}
