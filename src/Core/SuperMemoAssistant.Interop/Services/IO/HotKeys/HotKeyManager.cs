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
// Created On:   2019/03/02 18:29
// Modified On:  2020/01/12 12:14
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Services.Configuration;
using SuperMemoAssistant.Services.IO.Keyboard;
using SuperMemoAssistant.Sys.Collections;
using SuperMemoAssistant.Sys.IO.Devices;
using SuperMemoAssistant.Sys.Threading;

namespace SuperMemoAssistant.Services.IO.HotKeys
{
  public class HotKeyManager
  {
    #region Constants & Statics

    public static HotKeyManager Instance { get; } = new HotKeyManager();

    #endregion




    #region Properties & Fields - Non-Public

    private readonly ConcurrentDictionary<string, HotKeyData> _idDataMap      = new ConcurrentDictionary<string, HotKeyData>();
    private readonly ConcurrentDictionary<HotKey, HotKeyData> _hotKeyDataMap  = new ConcurrentDictionary<HotKey, HotKeyData>();
    private readonly ConcurrentBiDictionary<string, HotKey>   _defaultHotKeys = new ConcurrentBiDictionary<string, HotKey>();
    private readonly ConcurrentBiDictionary<string, HotKey>   _userHotKeyMap  = new ConcurrentBiDictionary<string, HotKey>();

    private readonly DelayedTask              _delayedTask;
    private          ConfigurationServiceBase _cfgSvc;
    private          HotKeyCfg                _config;
    private          IKeyboardHookService     _kbHookSvc;

    #endregion




    #region Constructors

    private HotKeyManager()
    {
      //if (_cfgSvc != null)
      //Initialize();

      _delayedTask = new DelayedTask(SaveConfig);
    }

    #endregion




    #region Properties & Fields - Public

    public IEnumerable<HotKeyData> HotKeys => _idDataMap.Values;

    #endregion




    #region Methods Impl

    public override string ToString()
    {
      return "HotKeys";
    }

    #endregion




    #region Methods

    public HotKeyManager Initialize(ConfigurationServiceBase cfgSvc, IKeyboardHookService kbHookSvc)
    {
      _cfgSvc    = cfgSvc;
      _kbHookSvc = kbHookSvc;

      if (_cfgSvc == null)
        throw new InvalidOperationException($"_cfgSvc cannot be null during {nameof(HotKeyManager)} initialization");

      if (_config != null)
        throw new InvalidOperationException($"{nameof(HotKeyManager)} is already initialized");

      _config = _cfgSvc.Load<HotKeyCfg>().Result ?? new HotKeyCfg();

      foreach (var kvp in _config.HotKeyMap)
        if (_userHotKeyMap.Reverse.ContainsKey(kvp.Value) == false)
          _userHotKeyMap[kvp.Key] = kvp.Value;

      return this;
    }

    public HotKeyManager RegisterGlobal(string      id,
                                        string      description,
                                        HotKeyScope scope,
                                        HotKey      defaultHotKey,
                                        Action      callback,
                                        bool        enabled = true)
    {
      Register(id, description, scope, defaultHotKey, callback, enabled);

      return this;
    }

    public HotKeyManager RegisterLocal(string id, string description, HotKey defaultHotKey, Action callback = null, bool enabled = true)
    {
      Register(id, description, null, defaultHotKey, callback, enabled);

      return this;
    }

    public HotKeyManager Enable(string id)
    {
      if (_config == null)
        throw new InvalidOperationException($"{nameof(HotKeyManager)} must be initialized");

      if (_idDataMap.ContainsKey(id) == false)
        throw new ArgumentException($"Hotkey {id} isn't registered");

      var hkData = _idDataMap[id];

      if (hkData.Enabled)
        return this;

      hkData.Enabled = true;

      if (hkData.IsGlobal)
        _kbHookSvc.RegisterHotKey(hkData.ActualHotKey, hkData.Callback);

      LogTo.Debug($"Hotkey {id} is now enabled.");

      return this;
    }

    public HotKeyManager Disable(string id)
    {
      if (_config == null)
        throw new InvalidOperationException($"{nameof(HotKeyManager)} must be initialized");

      if (_idDataMap.ContainsKey(id) == false)
        throw new ArgumentException($"Hotkey {id} isn't registered");

      var hkData = _idDataMap[id];

      if (hkData.Enabled == false)
        return this;

      hkData.Enabled = false;

      if (hkData.IsGlobal)
        _kbHookSvc.UnregisterHotKey(hkData.ActualHotKey);

      LogTo.Debug($"Hotkey {id} is now disabled.");

      return this;
    }

    public HotKeyData Match(HotKey hotKey) => _hotKeyDataMap.SafeGet(hotKey);

    private void HotKeyChanged(
      HotKeyData hkData,
      HotKey     actualBefore,
      HotKey     actualAfter)
    {
      if (actualBefore != null)
      {
        _hotKeyDataMap.TryRemove(actualBefore, out _);
        _userHotKeyMap.Reverse.Remove(actualBefore);

        if (hkData.IsGlobal)
          _kbHookSvc.UnregisterHotKey(actualBefore);
      }

      if (actualAfter != null)
      {
        if (_hotKeyDataMap.ContainsKey(actualAfter))
          throw new ArgumentException("Hotkey already used");

        // Remap hotkey
        _hotKeyDataMap[actualAfter] = hkData;

        // Remove ghost user hotkey if necessary, update hotkey
        _userHotKeyMap.Reverse.Remove(actualAfter);
        _userHotKeyMap[hkData.Id] = actualAfter;

        if (hkData.IsGlobal)
          _kbHookSvc.RegisterHotKey(actualAfter, hkData.Callback);
      }

      LogTo.Debug($"Hotkey {hkData.Id} is now bound to {actualAfter}.");

      _delayedTask.Trigger(1000);
    }

    private void Register(
      string       id,
      string       description,
      HotKeyScope? scope,
      HotKey       defaultHotKey,
      Action       callback,
      bool         enabled)
    {
      if (_config == null)
        throw new InvalidOperationException($"{nameof(HotKeyManager)} must be initialized");

      if (_idDataMap.ContainsKey(id))
        throw new ArgumentException($"Hotkey id {id} is already registered");

      if (_defaultHotKeys.Reverse.ContainsKey(defaultHotKey))
        throw new ArgumentException($"Default hotkey for {id} is already used by {_defaultHotKeys.Reverse[defaultHotKey]}");

      var hkData = CreateHotKeyData(id, description, scope != null, defaultHotKey, callback, enabled);

      _idDataMap[id]      = hkData;
      _defaultHotKeys[id] = defaultHotKey;

      if (hkData.ActualHotKey != null)
        _hotKeyDataMap[hkData.ActualHotKey] = hkData;

      if (enabled && scope != null)
        _kbHookSvc.RegisterHotKey(hkData.ActualHotKey, callback, scope.Value);

      LogTo.Debug($"Assigned default hotkey {defaultHotKey} to {id} ({description}).");
    }

    private HotKeyData CreateHotKeyData(
      string id,
      string description,
      bool   global,
      HotKey defaultHotKey,
      Action callback,
      bool   enabled)
    {
      HotKey actualHotKey = null;

      if (_userHotKeyMap.ContainsKey(id))
        actualHotKey = _userHotKeyMap[id];

      else if (_hotKeyDataMap.ContainsKey(defaultHotKey) == false)
        actualHotKey = defaultHotKey;

      var hkData = new HotKeyData(
        id,
        description,
        enabled,
        global,
        defaultHotKey,
        actualHotKey,
        callback);

      hkData.HotKeyChanged += HotKeyChanged;

      return hkData;
    }

    private void SaveConfig()
    {
      _config.HotKeyMap = new Dictionary<string, HotKey>(_userHotKeyMap);

      _cfgSvc.Save(_config).Wait();
    }

    #endregion
  }
}
