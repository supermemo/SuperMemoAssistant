using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant
{
  public class CoreCfg
  {
    public bool TrustHintAddresses { get; set; } = true;
    public Dictionary<string, int> PatternsHintAddresses { get; set; } = new Dictionary<string, int>();
  }
}
