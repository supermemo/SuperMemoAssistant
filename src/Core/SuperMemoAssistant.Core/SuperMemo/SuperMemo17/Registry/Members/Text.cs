using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;
using SuperMemoAssistant.Sys;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Members
{
  public class Text : RegistryMemberBase, IText
  {
    public Text(int id, RegMemElem mem, RegRtElem rt)
      : base(id, mem, rt)
    {

    }

    public string Name => RtxValue;

    public string GetFilePath()
    {
      return GetFilePath(".htm");
    }

    public async Task<string> GetContentAsync()
    {
      switch (LinkType)
      {
        case RegistryLinkType.Deleted:
          return null;

        case RegistryLinkType.Rtx:
          return RtxValue;

        case RegistryLinkType.FileAndRtx:
          return await Task.Run(() => File.ReadAllText(GetFilePath()));

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
