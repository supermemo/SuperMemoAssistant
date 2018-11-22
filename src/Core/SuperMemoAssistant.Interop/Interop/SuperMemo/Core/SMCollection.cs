using System;
using System.Configuration;

namespace SuperMemoAssistant.Interop.SuperMemo.Core
{
  [Serializable]
  public class SMCollection
  {
    public SMCollection(string name, string path)
      : this(name, path, DateTime.MinValue)
    {
    }

    public SMCollection(string name, string path, DateTime lastOpen)
    {
      Name = name;
      Path = path;
      LastOpen = lastOpen;
    }

    public string Name { get; set; }
    public string Path { get; set; }
    public DateTime LastOpen { get; set; }
  }
}
