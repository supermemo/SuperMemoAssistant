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

#endregion




namespace SuperMemoAssistant.Plugins
{
  using System.Linq;
  using System.Threading.Tasks;

  public partial class SMAPluginManager
  {
    #region Constants & Statics

    public override int PluginStopTimeout => 3000;

#if DEBUG
    public override int PluginConnectTimeout => 300000;
#else
    public override int PluginConnectTimeout => 10000;
#endif

    #endregion




    #region Methods

    /// <summary>Start plugin <paramref name="packageId" /></summary>
    /// <param name="packageId">The plugin's package id to start</param>
    /// <returns>Success of operation</returns>
    public async Task<bool> StartPluginAsync(string packageId)
    {
      var pluginInstance = AllPlugins.FirstOrDefault(p => p.Package.Id == packageId);

      if (pluginInstance == null)
        return false;

      return await StartPlugin(pluginInstance).ConfigureAwait(false);
    }

    #endregion
  }
}
