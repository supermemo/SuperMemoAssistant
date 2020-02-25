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
// Modified On:  2020/02/22 17:55
// Modified By:  Alexis

#endregion




using System;
using System.IO;
using System.Threading.Tasks;
using Anotar.Serilog;
using Process.NET.Windows;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Services.Configuration;
using SuperMemoAssistant.SMA.Configs;

namespace SuperMemoAssistant.SMA
{
  public partial class SMA
  {
    #region Properties & Fields - Public

    public CollectionCfg CollectionConfig { get; set; }
    public CoreCfg       CoreConfig       => Core.CoreConfig;

    #endregion




    #region Methods

    private void ApplySuperMemoWindowStyles()
    {
      if (CollectionConfig.CollapseElementWdwTitleBar)
        Task.Run(async () =>
        {
          await Task.Delay(4000).ConfigureAwait(false); // TODO: Fix this
          WindowStyling.MakeWindowTitleless(_sm.UI.ElementWdw.Handle);
        }).RunAsync();
    }

    private async Task LoadConfig(SMCollection collection)
    {
      Core.CollectionConfiguration = new CollectionConfigurationService(collection, "Core");

      // CollectionsCfg
      CollectionConfig = await Core.CollectionConfiguration.Load<CollectionCfg>() ?? new CollectionCfg();
    }

    public Task SaveConfig(bool sync)
    {
      try
      {
        var tasks = new[]
        {
          Core.Configuration.Save<CoreCfg>(CoreConfig),
          Core.CollectionConfiguration.Save<CollectionCfg>(CollectionConfig),
        };

        var task = Task.WhenAll(tasks);

        if (sync)
          task.Wait();

        return task;
      }
      catch (IOException ex)
      {
        if (ex.Message.StartsWith("The process cannot access the file", StringComparison.OrdinalIgnoreCase))
          LogTo.Warning(ex, "Failed to save config files in SMA.SaveConfig");

        else
          LogTo.Error(ex, "Failed to save config files in SMA.SaveConfig");

        return Task.CompletedTask;
      }
    }

    #endregion
  }
}
