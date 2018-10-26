using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Extensions
{
  public static class AssemblyEx
  {

    public static Guid GetAssemblyGuid()
    {
      var assembly = Assembly.GetExecutingAssembly();
      var guidAttr = assembly.GetCustomAttributes(typeof(GuidAttribute),
                                                  true);

      var guidStr = ((GuidAttribute)guidAttr.FirstOrDefault())?.Value
        ?? throw new NullReferenceException("GUID can't be null");

      return Guid.Parse(guidStr);
    }

    public static string GetAssemblyVersion()
    {
      var assembly = Assembly.GetExecutingAssembly();
      var fvi      = FileVersionInfo.GetVersionInfo(assembly.Location);

      return fvi.FileVersion;
    }
  }
}
