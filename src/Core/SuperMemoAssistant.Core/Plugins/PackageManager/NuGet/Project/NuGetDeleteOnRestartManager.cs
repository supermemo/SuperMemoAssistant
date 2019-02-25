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
// Created On:   2019/01/25 16:12
// Modified On:  2019/01/25 16:13
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using NuGet.PackageManagement;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;

namespace SuperMemoAssistant.Plugins.PackageManager.NuGet.Project
{
  internal class NuGetDeleteOnRestartManager : IDeleteOnRestartManager
  {
    #region Methods Impl

    /// <inheritdoc />
    public IReadOnlyList<string> GetPackageDirectoriesMarkedForDeletion()
    {
      return new List<string>();
    }

    /// <inheritdoc />
    public void CheckAndRaisePackageDirectoriesMarkedForDeletion()
    {
      PackagesMarkedForDeletionFound?.Invoke(this, null);
    }

    /// <inheritdoc />
    public void MarkPackageDirectoryForDeletion(PackageIdentity      package,
                                                string               packageDirectory,
                                                INuGetProjectContext projectContext) { }

    /// <inheritdoc />
    public void DeleteMarkedPackageDirectories(INuGetProjectContext projectContext) { }

    #endregion




    #region Events

    /// <inheritdoc />
    public event EventHandler<PackagesMarkedForDeletionEventArgs> PackagesMarkedForDeletionFound;

    #endregion
  }
}
