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
// Created On:   2020/02/12 22:20
// Modified On:  2020/02/12 22:20
// Modified By:  Alexis

#endregion




using System.Threading.Tasks;
using SuperMemoAssistant.Exceptions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Services.UI.Extensions;
using SuperMemoAssistant.SMA.Configs;

namespace SuperMemoAssistant
{
  public partial class App
  {
    #region Methods

    private Task<bool> LoadConfigs(out NativeDataCfg nativeDataCfg, out CoreCfg coreCfg)
    {
      nativeDataCfg = LoadNativeDataConfig().Result;
      coreCfg    = LoadCoreConfig().Result;

      if (nativeDataCfg == null || coreCfg == null)
        return Task.FromResult(false);

      return Task.FromResult(true);
    }

    private async Task<CoreCfg> LoadCoreConfig()
    {
      try
      {
        return await SMA.Core.Configuration.Load<CoreCfg>()
                        .ConfigureAwait(false) ?? new CoreCfg();
      }
      catch (SMAException)
      {
        await "Failed to open CoreCfg.json. Make sure file is unlocked and try again.".ErrorMsgBox();

        return null;
      }
    }

    private async Task<NativeDataCfg> LoadNativeDataConfig()
    {
      try
      {
        return await SMA.Core.Configuration.Load<NativeDataCfg>(SMAExecutableInfo.Instance.DirectoryPath)
                        .ConfigureAwait(false);
      }
      catch (SMAException)
      {
        await "Failed to load native data config file.".ErrorMsgBox();

        return null;
      }
    }

    #endregion
  }
}
