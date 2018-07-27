using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Files
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct RegRtElem
  {
    public byte[] value;
    public int no;
    public byte unknown;
  }
}
