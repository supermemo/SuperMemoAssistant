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
// Created On:   2019/09/03 18:08
// Modified On:  2020/01/11 20:46
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using Anotar.Serilog;
using AsyncEvent;
using Process.NET;
using SuperMemoAssistant.Exceptions;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.SuperMemo;
using SuperMemoAssistant.SuperMemo.Common;
using SuperMemoAssistant.SuperMemo.Common.Content.Layout;
using SuperMemoAssistant.SuperMemo.SuperMemo17;
using SuperMemoAssistant.Sys;
using SuperMemoAssistant.Sys.IO;

namespace SuperMemoAssistant.SMA
{
  /// <summary>
  ///   Wrapper around a SM management instance that handles SuperMemo App lifecycle events
  ///   (start, exit, ...) and provides a safe interface to interact with SuperMemo
  /// </summary>
  public partial class SMA
    : PerpetualMarshalByRefObject,
      ISuperMemoAssistant,
      IDisposable
  {
    #region Constants & Statics

    public static SMA Instance { get; } = new SMA();

    #endregion




    #region Properties & Fields - Non-Public

    protected SuperMemoCore _sm;

    #endregion




    #region Constructors

    /// <summary>
    ///   Create an instance of the wrapper that will start a SM instance and attach the
    ///   management engine.
    /// </summary>
    protected SMA()
    {
      Core.SMA = this;
    }


    /// <inheritdoc />
    public virtual void Dispose()
    {
      _sm?.Dispose();
    }

    #endregion




    #region Properties & Fields - Public

    public IProcess SMProcess => _sm?.SMProcess;

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public IEnumerable<string> Layouts => LayoutManager.Instance.Layouts
                                                       .Select(l => l.Name)
                                                       .OrderBy(n => n)
                                                       .ToList();

    public ISuperMemo SM => _sm;

    #endregion




    #region Methods

    //
    // Collection loading management

    public async Task<bool> Start(SMCollection collection)
    {
      try
      {
        if (_sm != null)
          throw new InvalidOperationException("_sm is already instantiated");

        LoadConfig(collection);
        var nativeData = CheckSuperMemoExecutable();

        _sm = InstantiateSuperMemo(collection, nativeData.SMVersion);

        // TODO: Move somewhere else
        _sm.UI.ElementWdw.OnAvailable += OnSuperMemoWindowsAvailable;

        await _sm.Start(nativeData);
        // TODO: Ensure opened collection (windows title) matches parameter
      }
      catch (Exception ex)
      {
        if (ex is SMAException)
          LogTo.Warning(ex, "Failed to start SM.");

        else
          LogTo.Error(ex, "Failed to start SM.");

        _sm?.Dispose();
        _sm = null;

        try
        {
          if (OnSMStoppedEvent != null)
            await OnSMStoppedEvent.InvokeAsync(this,
                                               new SMProcessArgs(_sm, null)).ConfigureAwait(true);
        }
        catch (Exception pluginEx)
        {
          LogTo.Error(pluginEx,
                      "Exception while notifying plugins OnSMStoppedEvent.");
        }

        // TODO: Handle exception

        return false;
      }

      return true;
    }

    private SuperMemoCore InstantiateSuperMemo(SMCollection collection,
                                               Version      smVersion)
    {
      if (SM17.Versions.Contains(smVersion))
        return new SM17(collection,
                        StartupConfig.SMBinPath);

      throw new SMAException($"Unsupported SM version {smVersion}");
    }

    private NativeData CheckSuperMemoExecutable()
    {
      var nativeDataCfg = LoadNativeDataConfig();
      var smFile        = new FilePath(StartupConfig.SMBinPath);

      if (smFile.Exists() == false)
        throw new SMAException(
          $"Invalid file path for sm executable file: '{StartupConfig.SMBinPath}' could not be found. SMA cannot continue.");

      if (smFile.HasPermission(FileIOPermissionAccess.Read) == false)
        throw new SMAException($"SMA needs read access to execute SM executable at {smFile.FullPath}.");

      if (smFile.IsLocked())
        throw new SMAException($"{smFile.FullPath} is locked. Make sure it isn't already running.");

      var smFileCrc32 = FileEx.GetCrc32(smFile.FullPath);
      var nativeData  = nativeDataCfg.SafeGet(smFileCrc32.ToUpper(CultureInfo.InvariantCulture));

      if (nativeData == null)
        throw new SMAException($"Unknown SM executable version with crc32 {smFileCrc32}.");

      LogTo.Information($"SuperMemo version {nativeData.SMVersion} detected");

      return nativeData;
    }

    #endregion
  }
}
