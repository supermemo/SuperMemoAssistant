using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Interop.SuperMemo.Registry.Members
{
  public interface IImage : IRegistryMember
  {
    Task<Bitmap> GetContentAsync();
  }
}
