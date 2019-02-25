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
// Created On:   2019/02/25 14:17
// Modified On:  2019/02/25 15:43
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Forge.Forms;
using Forge.Forms.Annotations;
using Microsoft.Win32;
using Newtonsoft.Json;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Sys.ComponentModel;

namespace SuperMemoAssistant.SMA
{
  [Form(Mode = DefaultFields.None)]
  [Title("Settings",
    IsVisible = "{Env DialogHostContext}")]
  [DialogAction("save",
    "Save",
    IsDefault = true,
    Validates = true)]
  public class StartupCfg : IActionHandler, INotifyPropertyChangedEx
  {
    #region Properties & Fields - Public

    [Field(Name = "SM Binary Path")]
    [Action("BrowseFile",
      "Browse",
      Placement = Placement.Inline)]
    public string SMBinPath { get; set; } = SMConst.BinPath;

    [Field(Name = "Skip procedures pattern scanning (trust hint addresses)")]
    public bool TrustHintAddresses { get;                                  set; } = false;
    public Dictionary<string, int>            PatternsHintAddresses { get; set; } = new Dictionary<string, int>();
    public ObservableCollection<SMCollection> Collections           { get; set; } = new ObservableCollection<SMCollection>();

    #endregion




    #region Properties Impl - Public

    [JsonIgnore]
    public bool IsChanged { get; set; }

    #endregion




    #region Methods Impl

    public void HandleAction(IActionContext actionContext)
    {
      var action = actionContext.Action as string;

      switch (action)
      {
        case "BrowseFile":
          OpenFileDialog dlg = new OpenFileDialog
          {
            DefaultExt = ".exe",
            Filter     = "Executable files (*.exe)|*.exe|All files (*.*)|*.*"
          };

          SMBinPath = dlg.ShowDialog().GetValueOrDefault(false)
            ? dlg.FileName
            : SMBinPath;
          break;
      }
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
