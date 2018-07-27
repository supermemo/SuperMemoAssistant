using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperMemoAssistant.Sys;

namespace SuperMemoAssistant
{
  public static class ModuleInitializer
  {
    public static void Initialize()
    {
      InitOnLoad.Initialize();
    }
  }
}
