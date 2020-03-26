using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using SuperMemoAssistant.Services;

namespace SuperMemoAssistant.Extensions
{
  /// <summary>
  /// Extension methods for <see cref="FileDialog" />
  /// </summary>
  public static class FileDialogEx
  {
    /// <summary>
    /// Shows the dialog with <paramref name="ownerHwnd"/> as the owner
    /// </summary>
    /// <param name="dlg">The dialog to show</param>
    /// <param name="ownerHwnd">The owner window</param>
    /// <returns>Dialog result</returns>
    public static bool RunDialog(this FileDialog dlg, IntPtr ownerHwnd)
    {
      return (bool)dlg.GetType()
                      .GetMethod("RunDialog", BindingFlags.NonPublic | BindingFlags.Instance)
                      .Invoke(dlg, new object[] { ownerHwnd });
    }

    /// <summary>
    /// Shows the dialog with SuperMemo Element window as the owner
    /// </summary>
    /// <param name="dlg">The dialog to show</param>
    /// <returns>Dialog result</returns>
    public static bool RunDialogInSuperMemo(this FileDialog dlg)
    {
      return dlg.RunDialog(Svc.SM.UI.ElementWdw.Handle);
    }
  }
}
