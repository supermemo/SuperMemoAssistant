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
// Modified On:  2020/03/11 17:58
// Modified By:  Alexis

#endregion




using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Windows.UI.Notifications;

namespace SuperMemoAssistant.Sys.Windows
{
  // https://github.com/WindowsNotifications/desktop-toasts/
  public class DesktopNotificationManager
  {
    #region Constants & Statics

    public const string TOAST_ACTIVATED_LAUNCH_ARG = "-ToastActivated";

    private static bool   _registeredAumidAndComServer;
    private static string _aumid;
    private static bool   _registeredActivator;

    /// <summary>
    ///   Gets the <see cref="DesktopNotificationHistory" /> object. You must have called
    ///   <see cref="RegisterActivator{T}" /> first (and also
    ///   <see cref="RegisterAumidAndComServer{T}(string)" /> if you're a classic Win32 app), or this
    ///   will throw an exception.
    /// </summary>
    public static DesktopNotificationHistory History
    {
      get
      {
        EnsureRegistered();

        return new DesktopNotificationHistory(_aumid);
      }
    }

    /// <summary>
    ///   Gets a boolean representing whether http images can be used within toasts. This is
    ///   true if running under Desktop Bridge.
    /// </summary>
    public static bool CanUseHttpImages
    {
      get { return DesktopBridgeHelpers.IsRunningAsUwp(); }
    }

    #endregion




    #region Methods

    /// <summary>
    ///   If not running under the Desktop Bridge, you must call this method to register your
    ///   AUMID with the Compat library and to register your COM CLSID and EXE in LocalServer32
    ///   registry. Feel free to call this regardless, and we will no-op if running under Desktop
    ///   Bridge. Call this upon application startup, before calling any other APIs.
    /// </summary>
    /// <param name="aumid">An AUMID that uniquely identifies your application.</param>
    public static void RegisterAumidAndComServer<T>(string aumid)
      where T : NotificationActivator
    {
      if (string.IsNullOrWhiteSpace(aumid))
        throw new ArgumentException("You must provide an AUMID.", nameof(aumid));

      // If running as Desktop Bridge
      if (DesktopBridgeHelpers.IsRunningAsUwp())
      {
        // Clear the AUMID since Desktop Bridge doesn't use it, and then we're done.
        // Desktop Bridge apps are registered with platform through their manifest.
        // Their LocalServer32 key is also registered through their manifest.
        _aumid                       = null;
        _registeredAumidAndComServer = true;
        return;
      }

      _aumid = aumid;

      String exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
      RegisterComServer<T>(exePath);

      _registeredAumidAndComServer = true;
    }

    private static void RegisterComServer<T>(String exePath)
      where T : NotificationActivator
    {
      // We register the EXE to start up when the notification is activated
      string regString = String.Format("SOFTWARE\\Classes\\CLSID\\{{{0}}}\\LocalServer32", typeof(T).GUID);
      var    key       = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(regString);

      // Include a flag so we know this was a toast activation and should wait for COM to process
      // We also wrap EXE path in quotes for extra security
      key.SetValue(null, '"' + exePath + '"' + " " + TOAST_ACTIVATED_LAUNCH_ARG);
    }

    /// <summary>
    ///   Registers the activator type as a COM server client so that Windows can launch your
    ///   activator.
    /// </summary>
    /// <typeparam name="T">
    ///   Your implementation of NotificationActivator. Must have GUID and
    ///   ComVisible attributes on class.
    /// </typeparam>
    public static void RegisterActivator<T>()
      where T : NotificationActivator
    {
      // Register type
      var regService = new RegistrationServices();

      regService.RegisterTypeForComClients(
        typeof(T),
        RegistrationClassContext.LocalServer,
        RegistrationConnectionType.MultipleUse);

      _registeredActivator = true;
    }

    /// <summary>
    ///   Creates a toast notifier. You must have called <see cref="RegisterActivator{T}" />
    ///   first (and also <see cref="RegisterAumidAndComServer{T}(string)" /> if you're a classic
    ///   Win32 app), or this will throw an exception.
    /// </summary>
    /// <returns></returns>
    public static ToastNotifier CreateToastNotifier()
    {
      EnsureRegistered();

      if (_aumid != null)
        // Non-Desktop Bridge
        return ToastNotificationManager.CreateToastNotifier(_aumid);
      else
        // Desktop Bridge
        return ToastNotificationManager.CreateToastNotifier();
    }

    private static void EnsureRegistered()
    {
      // If not registered AUMID yet
      if (!_registeredAumidAndComServer)
      {
        // Check if Desktop Bridge
        if (DesktopBridgeHelpers.IsRunningAsUwp())
          // Implicitly registered, all good!
          _registeredAumidAndComServer = true;

        else
          // Otherwise, incorrect usage
          throw new Exception("You must call RegisterAumidAndComServer first.");
      }

      // If not registered activator yet
      if (!_registeredActivator)
        // Incorrect usage
        throw new Exception("You must call RegisterActivator first.");
    }

    #endregion




    /// <summary>
    ///   Code from
    ///   https://github.com/qmatteoq/DesktopBridgeHelpers/edit/master/DesktopBridge.Helpers/Helpers.cs
    /// </summary>
    private class DesktopBridgeHelpers
    {
      #region Constants & Statics

      private const long APPMODEL_ERROR_NO_PACKAGE = 15700L;

      private static bool? _isRunningAsUwp;

      private static bool IsWindows7OrLower
      {
        get
        {
          int    versionMajor = Environment.OSVersion.Version.Major;
          int    versionMinor = Environment.OSVersion.Version.Minor;
          double version      = versionMajor + (double)versionMinor / 10;
          return version <= 6.1;
        }
      }

      #endregion




      #region Methods

      [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder packageFullName);

      public static bool IsRunningAsUwp()
      {
        if (_isRunningAsUwp == null)
        {
          if (IsWindows7OrLower)
          {
            _isRunningAsUwp = false;
          }
          else
          {
            int           length = 0;
            StringBuilder sb     = new StringBuilder(0);
            GetCurrentPackageFullName(ref length, sb);

            sb = new StringBuilder(length);
            int result = GetCurrentPackageFullName(ref length, sb);

            _isRunningAsUwp = result != APPMODEL_ERROR_NO_PACKAGE;
          }
        }

        return _isRunningAsUwp.Value;
      }

      #endregion
    }
  }

  /// <summary>
  ///   Manages the toast notifications for an app including the ability the clear all toast
  ///   history and removing individual toasts.
  /// </summary>
  public sealed class DesktopNotificationHistory
  {
    #region Properties & Fields - Non-Public

    private readonly string                   _aumid;
    private readonly ToastNotificationHistory _history;

    #endregion




    #region Constructors

    /// <summary>
    ///   Do not call this. Instead, call <see cref="DesktopNotificationManager.History" /> to
    ///   obtain an instance.
    /// </summary>
    /// <param name="aumid"></param>
    internal DesktopNotificationHistory(string aumid)
    {
      _aumid   = aumid;
      _history = ToastNotificationManager.History;
    }

    #endregion




    #region Methods

    /// <summary>Removes all notifications sent by this app from action center.</summary>
    public void Clear()
    {
      if (_aumid != null)
        _history.Clear(_aumid);
      else
        _history.Clear();
    }

    /// <summary>Gets all notifications sent by this app that are currently still in Action Center.</summary>
    /// <returns>A collection of toasts.</returns>
    public IReadOnlyList<ToastNotification> GetHistory()
    {
      return _aumid != null ? _history.GetHistory(_aumid) : _history.GetHistory();
    }

    /// <summary>Removes an individual toast, with the specified tag label, from action center.</summary>
    /// <param name="tag">The tag label of the toast notification to be removed.</param>
    public void Remove(string tag)
    {
      if (_aumid != null)
        _history.Remove(tag, string.Empty, _aumid);
      else
        _history.Remove(tag);
    }

    /// <summary>
    ///   Removes a toast notification from the action using the notification's tag and group
    ///   labels.
    /// </summary>
    /// <param name="tag">The tag label of the toast notification to be removed.</param>
    /// <param name="group">The group label of the toast notification to be removed.</param>
    public void Remove(string tag, string group)
    {
      if (_aumid != null)
        _history.Remove(tag, @group, _aumid);
      else
        _history.Remove(tag, @group);
    }

    /// <summary>
    ///   Removes a group of toast notifications, identified by the specified group label, from
    ///   action center.
    /// </summary>
    /// <param name="group">The group label of the toast notifications to be removed.</param>
    public void RemoveGroup(string group)
    {
      if (_aumid != null)
        _history.RemoveGroup(@group, _aumid);
      else
        _history.RemoveGroup(@group);
    }

    #endregion
  }

  /// <summary>Apps must implement this activator to handle notification activation.</summary>
  public abstract class NotificationActivator : NotificationActivator.INotificationActivationCallback
  {
    #region Methods Impl

    public void Activate(string appUserModelId, string invokedArgs, NotificationUserInputData[] data, uint dataCount)
    {
      OnActivated(invokedArgs, new NotificationUserInput(data), appUserModelId);
    }

    #endregion




    #region Methods Abs

    /// <summary>
    ///   This method will be called when the user clicks on a foreground or background
    ///   activation on a toast. Parent app must implement this method.
    /// </summary>
    /// <param name="arguments">
    ///   The arguments from the original notification. This is either the
    ///   launch argument if the user clicked the body of your toast, or the arguments from a button
    ///   on your toast.
    /// </param>
    /// <param name="userInput">Text and selection values that the user entered in your toast.</param>
    /// <param name="appUserModelId">Your AUMID.</param>
    public abstract void OnActivated(string arguments, NotificationUserInput userInput, string appUserModelId);

    #endregion




    // These are the new APIs for Windows 10




    #region NewAPIs

    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct NotificationUserInputData
    {
      [MarshalAs(UnmanagedType.LPWStr)] public string Key;

      [MarshalAs(UnmanagedType.LPWStr)] public string Value;
    }

    [ComImport]
    [Guid("53E31837-6600-4A81-9395-75CFFE746F94")]
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface INotificationActivationCallback
    {
      void Activate(
        [In][MarshalAs(UnmanagedType.LPWStr)] string appUserModelId,
        [In][MarshalAs(UnmanagedType.LPWStr)] string invokedArgs,
        [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
        NotificationUserInputData[] data,
        [In][MarshalAs(UnmanagedType.U4)] uint dataCount);
    }

    #endregion
  }

  /// <summary>
  ///   Text and selection values that the user entered on your notification. The Key is the
  ///   ID of the input, and the Value is what the user entered.
  /// </summary>
  public class NotificationUserInput : IReadOnlyDictionary<string, string>
  {
    #region Properties & Fields - Non-Public

    private readonly NotificationActivator.NotificationUserInputData[] _data;

    #endregion




    #region Constructors

    internal NotificationUserInput(NotificationActivator.NotificationUserInputData[] data)
    {
      _data = data;
    }

    #endregion




    #region Properties Impl - Public

    public int Count => _data.Length;

    public string this[string key] => _data.First(i => i.Key == key).Value;

    public IEnumerable<string> Keys => _data.Select(i => i.Key);

    public IEnumerable<string> Values => _data.Select(i => i.Value);

    #endregion




    #region Methods Impl

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
      return _data.Select(i => new KeyValuePair<string, string>(i.Key, i.Value)).GetEnumerator();
    }

    public bool ContainsKey(string key)
    {
      return _data.Any(i => i.Key == key);
    }

    public bool TryGetValue(string key, out string value)
    {
      foreach (var item in _data)
        if (item.Key == key)
        {
          value = item.Value;
          return true;
        }

      value = null;
      return false;
    }

    #endregion
  }
}
