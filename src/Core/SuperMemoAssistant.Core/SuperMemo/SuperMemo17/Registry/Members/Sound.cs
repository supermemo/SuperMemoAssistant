using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Members
{
  public class Sound : RegistryMemberBase, ISound
  {
    public Sound(int id, RegMemElem mem, RegRtElem rt)
      : base(id, mem, rt)
    {

    }

    public string Name => RtxValue;

    public string GetFilePath()
    {
      //switch ()
      return null; // GetFilePath(".mp3");
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
