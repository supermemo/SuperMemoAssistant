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
// Created On:   2019/01/20 08:05
// Modified On:  2019/01/20 08:29
// Modified By:  Alexis

#endregion




using System;
using NuGet.Common;
using NuGet.Configuration;

namespace SuperMemoAssistant.Plugins.PackageManager.NuGet
{
  /// <summary>Original from: https://github.com/Wyamio/Wyam/ Copyright (c) 2014 Dave Glick</summary>
  internal class MachineWideSettings : IMachineWideSettings
  {
    #region Properties & Fields - Non-Public

    private readonly Lazy<ISettings> _settings;

    ISettings IMachineWideSettings.Settings => _settings.Value;

    #endregion




    #region Constructors

    public MachineWideSettings()
    {
      string baseDirectory = NuGetEnvironment.GetFolderPath(NuGetFolderPath.MachineWideConfigDirectory);

      _settings = new Lazy<ISettings>(
        () => Settings.LoadMachineWideSettings(baseDirectory));
    }

    #endregion
  }
}
