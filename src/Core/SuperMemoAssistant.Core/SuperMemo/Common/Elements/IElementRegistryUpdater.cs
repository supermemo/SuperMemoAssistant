using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Sys.SparseClusteredArray;

namespace SuperMemoAssistant.SuperMemo.Common.Elements
{
  public interface IElementRegistryUpdater
  {
    void CommitFromFiles(SMCollection collection);

    void CommitFromMemory(SparseClusteredArray<byte> contentsSCA, SparseClusteredArray<byte> elementsSCA);
  }
}
