using System.IO;
using System.Runtime.InteropServices;

namespace SuperMemoAssistant.Extensions
{
  public static class BinaryReaderEx
  {
    public static bool FindSequence(this BinaryReader r, byte[] seq)
    {
      int seqPos = 0;
      int seqLength = seq.Length;

      try
      {
        while (seqPos != seqLength)
        {
          if (r.BaseStream.Position >= r.BaseStream.Length)
            return false;

          if (r.ReadByte() == seq[seqPos])
            seqPos++;

          else
            seqPos = 0;
        }

        return true;
      }
      catch (EndOfStreamException)
      {
        return false;
      }
    }

    public static T ReadStruct<T>(this BinaryReader r)
    {
      byte[] rawData = r.ReadBytes(Marshal.SizeOf(typeof(T)));

      var pinnedRawData = GCHandle.Alloc(rawData,
                                         GCHandleType.Pinned);
      try
      {
        // Get the address of the data array
        var pinnedRawDataPtr = pinnedRawData.AddrOfPinnedObject();

        // overlay the data type on top of the raw data
        return (T)Marshal.PtrToStructure(pinnedRawDataPtr, typeof(T));
      }
      finally
      {
        // must explicitly release
        pinnedRawData.Free();
      }
    }
  }
}
