using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Members
{
  public class Image : RegistryMemberBase, IImage
  {
    public Image(int id, RegMemElem mem, RegRtElem rt)
      : base(id, mem, rt)
    {

    }

    public string Name => RtxValue;

    public string GetFilePath()
    {
      //switch ()
      return GetFilePath(".jpg");
    }

    public async Task<Bitmap> GetContentAsync()
    {
      switch (LinkType)
      {
        case RegistryLinkType.Deleted:
          return null;

        case RegistryLinkType.FileAndRtx:
          return await Task.Run(() => new Bitmap(GetFilePath()));

        case RegistryLinkType.Rtx:
        case RegistryLinkType.Rtf:
          break;
      }

      throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync()
    {
      throw new NotImplementedException();
    }

    public IEnumerable<IElement> GetLinkedElements()
    {
      throw new NotImplementedException();
    }

    public Task<bool> NeuralAsync()
    {
      throw new NotImplementedException();
    }

    public Task<bool> RenameAsync(string newName)
    {
      throw new NotImplementedException();
    }
  }
}
