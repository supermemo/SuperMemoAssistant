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
// Modified On:  2020/03/23 04:02
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

    private async Task<(bool success, NativeDataCfg nativeDataCfg, CoreCfg coreCfg)> LoadConfigs()
    {
      var nativeDataCfg = await LoadNativeDataConfig().ConfigureAwait(false);
      var coreCfg       = await LoadCoreConfig().ConfigureAwait(false);

      if (nativeDataCfg == null || coreCfg == null)
        return (false, null, null);

      return (true, nativeDataCfg, coreCfg);
    }

    private async Task<CoreCfg> LoadCoreConfig()
    {
      try
      {
        return await SMA.Core.Configuration
                        .Load<CoreCfg>()
                        .ConfigureAwait(false) ?? new CoreCfg();
      }
      catch (SMAException)
      {
        return null;
      }
    }

    private async Task<NativeDataCfg> LoadNativeDataConfig()
    {
      try
      {
        return await SMA.Core.Configuration
                        .Load<NativeDataCfg>(SMAExecutableInfo.Instance.DirectoryPath)
                        .ConfigureAwait(false);
      }
      catch (SMAException)
      {
        return null;
      }
    }

    #endregion
  }
}
