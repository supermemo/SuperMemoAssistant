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
// Created On:   2018/06/01 13:31
// Modified On:  2018/06/01 13:33
// Modified By:  Alexis

#endregion




using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.Plugins;

namespace SuperMemoAssistant.Services.Configuration
{
  public class ConfigurationService
  {
    #region Properties & Fields - Non-Public

    protected ISMAPlugin Plugin { get; }

    #endregion




    #region Constructors

    public ConfigurationService(ISMAPlugin plugin)
    {
      Plugin = plugin;
    }

    #endregion




    #region Methods

    public Task<T> Load<T>(string fileName = null)
    {
      // TODO: Implement fileName
      return Load<T>(Plugin);
    }

    public Task<bool> Save<T>(T config, string fileName = null)
    {
      // TODO: Implement fileName
      return Save<T>(Plugin, config);
    }


    private static async Task<T> Load<T>(ISMAPlugin requester)
    {
      try
      {
        using (var stream = EnsurePluginConf(requester, typeof(T), FileAccess.Read))
        using (var reader = new StreamReader(stream))
          return JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync().ConfigureAwait(false));
      }
      catch (Exception ex)
      {
        // TODO: Log
        return default(T);
      }
    }

    private static async Task<bool> Save<T>(ISMAPlugin requester, T config)
    {
      try
      {
        using (var stream = EnsurePluginConf(requester, typeof(T), FileAccess.Write))
        using (var writer = new StreamWriter(stream))
          await writer.WriteAsync(JsonConvert.SerializeObject(config, Formatting.Indented)).ConfigureAwait(false);

        return true;
      }
      catch (Exception ex)
      {
        // TODO: Log
        return false;
      }
    }

    private static FileStream EnsurePluginConf(ISMAPlugin plugin, Type confType, FileAccess fileAccess)
    {
      string filePath = Path.Combine(SMAConst.Paths.ConfigPath, plugin.Id.ToString("D"));

      if (!DirectoryEx.EnsureExists(filePath))
        return null;

      filePath = Path.Combine(filePath, confType.Name);

      return File.Open(filePath, fileAccess == FileAccess.Read ? FileMode.OpenOrCreate : FileMode.Create, fileAccess);
    }

    #endregion
  }
}
