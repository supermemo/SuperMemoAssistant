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
// Modified On:  2020/03/11 15:25
// Modified By:  Alexis

#endregion




using System.ComponentModel;
using Newtonsoft.Json;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Sys.Collections.Microsoft.EntityFrameworkCore.ChangeTracking;
using SuperMemoAssistant.Sys.ComponentModel;

namespace SuperMemoAssistant.SMA.Configs
{
  /// <summary>The main config file for SMA Core</summary>
  public class CoreCfg : INotifyPropertyChangedEx
  {
    #region Properties & Fields - Public

    public SuperMemoCfg SuperMemo { get; set; } = new SuperMemoCfg();
    public UpdateCfg    Updates   { get; set; } = new UpdateCfg();

    /// <summary>Whether user has agreed to the terms of license during the setup</summary>
    public bool HasAgreedToTermsOfLicense { get; set; } = false;

    /// <summary>Whether user has gone through the import collection setup step</summary>
    public bool HasImportedCollections { get; set; } = false;

    /// <summary>User's saved SM collections</summary>
    public ObservableHashSet<SMCollection> Collections { get; } = new ObservableHashSet<SMCollection>();

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    [JsonIgnore]
    public bool IsChanged { get; set; }

    #endregion




    #region Events

    /// <inheritdoc />
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
