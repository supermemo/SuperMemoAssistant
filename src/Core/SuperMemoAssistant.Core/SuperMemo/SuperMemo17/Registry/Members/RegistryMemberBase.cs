using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.Core;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Members
{
  public abstract class RegistryMemberBase : MarshalByRefObject
  {
    protected RegistryMemberBase(int id, RegMemElem mem, RegRtElem rt)
    {
      Id = id;

#if DEBUG
      System.Diagnostics.Debug.WriteLine("[{0} {1}] Creating", this.GetType().Name, Id);
#endif

      UseCount = SetDbg(mem.useCount, nameof(UseCount));

      LinkType = SetDbg((RegistryLinkType)mem.linkType, nameof(LinkType));

      RtxId = SetDbg(rt.no, nameof(RtxId));
      RtxOffset = SetDbg(mem.rtxOffset, nameof(RtxOffset));
      RtxLength = SetDbg(mem.rtxLength, nameof(RtxLength));

      if (rt.value != null)
        RtxValue = SetDbg(Encoding.UTF8.GetString(rt.value), nameof(RtxValue));

      SlotIdOrOffset = SetDbg(mem.slotIdOrOffset, nameof(SlotIdOrOffset));
      SlotLengthOrConceptGroupId = SetDbg(mem.slotLengthOrConceptGroup, nameof(SlotLengthOrConceptGroupId));

      Empty = SetDbg(RtxValue == null, nameof(Empty));
    }

    public void Update(RegMemElem mem, RegRtElem rt)
    {
#if DEBUG
      //System.Diagnostics.Debug.WriteLine("[{0} {1}] Updating", this.GetType().Name, Id);
#endif

      if (mem.rtxOffset == 0)
      {
        Empty = SetDbg(Empty, true, nameof(Empty));

        return;
      }

      UseCount = SetDbg(UseCount, mem.useCount, nameof(UseCount));

      LinkType = SetDbg(LinkType, (RegistryLinkType)mem.linkType, nameof(LinkType));

      RtxOffset = SetDbg(RtxOffset, mem.rtxOffset, nameof(RtxOffset));
      RtxLength = SetDbg(RtxLength, mem.rtxLength, nameof(RtxLength));

      if (rt.value != null)
      {
        RtxId = SetDbg(RtxId, rt.no, nameof(RtxId));
        RtxValue = SetDbg(RtxValue, Encoding.UTF8.GetString(rt.value), nameof(RtxValue));
      }

      SlotIdOrOffset = SetDbg(SlotIdOrOffset, mem.slotIdOrOffset, nameof(SlotIdOrOffset));
      SlotLengthOrConceptGroupId = SetDbg(SlotLengthOrConceptGroupId, mem.slotLengthOrConceptGroup, nameof(SlotLengthOrConceptGroupId));
    }

    protected T SetDbg<T>(T oldValue, T value, string name)
    {
#if DEBUG
      if (Object.Equals(oldValue, value) == false)
        System.Diagnostics.Debug.WriteLine("[{0} {1}] {2}: {3}", this.GetType().Name, Id, name, value);
#endif

      return value;
    }

    protected T SetDbg<T>(T value, string name)
    {
#if DEBUG
      System.Diagnostics.Debug.WriteLine("[{0} {1}] {2}: {3}", this.GetType().Name, Id, name, value);
#endif

      return value;
    }

    public string GetFilePath(string fileExt)
    {
      switch (LinkType)
      {
        case RegistryLinkType.FileAndRtx:
          SMCollection collection = SMA.Instance.Collection;

          return GetFilePathForSlotId(
            collection,
            SlotIdOrOffset,
            fileExt
          );

        default:
          return null;
      }
    }

    protected static string GetFilePathForSlotId(
      SMCollection collection,
      int slotId,
      string slotFileExt)
    {
      if (slotId <= 10)
        return collection.GetElementFilePath(String.Format("{0}.{1}", slotId, slotFileExt));

      List<int> folders = new List<int>();
      int nBranch = (int)Math.Floor((slotId - 1) / 10.0);

      do
      {
        folders.Add(((nBranch - 1) % 30) + 1);

        nBranch = (int)Math.Floor((nBranch - 1) / 30.0);
      } while (nBranch > 0);

      folders.Reverse();
      string folderPath = String.Join("\\", folders);

      return collection.GetElementFilePath(
        Path.Combine(
          folderPath,
          String.Format("{0}.{1}", slotId, slotFileExt)
        )
      );
    }

    public int Id { get; set; }

    public int UseCount { get; set; }

    public RegistryLinkType LinkType { get; set; }

    public int RtxId { get; set; }
    public int RtxOffset { get; set; }
    public int RtxLength { get; set; }
    public string RtxValue { get; set; }

    public int SlotIdOrOffset { get; set; }
    public int SlotLengthOrConceptGroupId { get; set; }

    public bool Empty { get; set; }
  }
}
