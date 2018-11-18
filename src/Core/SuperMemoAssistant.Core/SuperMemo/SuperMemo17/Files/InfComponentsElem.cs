using System.Runtime.InteropServices;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Files
{
  [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 29)]
  public unsafe struct InfComponentsHtml
  {
    public byte         unknown1;
    public short        left;
    public short        top;
    public short        right;
    public short        bottom;
    public byte         displayAt;
    public fixed byte   unknown2[5];
    public byte         isFullHtml;
    public fixed byte   unknown3[2];
    public int          registryId;
    public fixed byte   unknown4[7];
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 35)]
  public unsafe struct InfComponentsText
  {
    public byte         unknown1;
    public short        left;
    public short        top;
    public short        right;
    public short        bottom;
    public byte         displayAt;
    public fixed byte   unknown2[8];
    public int          registryId;
    public byte         textAlignment; //0: Left, 1: Center, 2: Right
    public byte         colorRed;
    public byte         colorGreen;
    public byte         colorBlue;
    public fixed byte   unknown4[9];
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 30)]
  public unsafe struct InfComponentsRtf
  {
    public byte         unknown1;
    public short        left;
    public short        top;
    public short        right;
    public short        bottom;
    public byte         displayAt;
    public fixed byte   unknown2[8];
    public int          registryId;
    public byte         unknown3;
    public byte         colorRed;
    public byte         colorGreen;
    public byte         colorBlue;
    public fixed byte   unknown4[4];
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 35)]
  public unsafe struct InfComponentsSpelling
  {
    public byte         unknown1;
    public short        left;
    public short        top;
    public short        right;
    public short        bottom;
    public byte         displayAt;
    public fixed byte   unknown2[8];
    public int          registryId;
    public fixed byte   unknown3[2];
    public byte         colorRed;
    public byte         colorGreen;
    public byte         colorBlue;
    public fixed byte   unknown4[8];
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 26)]
  public unsafe struct InfComponentsImage
  {
    public byte         unknown1;
    public short        left;
    public short        top;
    public short        right;
    public short        bottom;
    public byte         displayAt;
    public fixed byte   unknown2[8];
    public int          registryId;
    public byte         stretchType;
    public fixed byte   unknown3[3];
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 49)]
  public unsafe struct InfComponentsSound
  {
    public byte         unknown1;
    public short        left;
    public short        top;
    public short        right;
    public short        bottom;
    public byte         displayAt;
    public fixed byte   unknown2[8];
    public int          registryId;
    public fixed byte   unknown3[8];
    public byte         textAlignment;
    public byte         unknown4;
    public byte         colorRed;
    public byte         colorGreen;
    public byte         colorBlue;
    public byte         unknown5;
    public uint         extractStart;
    public uint         extractStop;
    public bool         isContinuous;
    public fixed byte   unknown6[2];
    public byte         playAt;
    public byte         panel;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 32)]
  public unsafe struct InfComponentsVideo
  {
    public byte         unknown1;
    public short        left;
    public short        top;
    public short        right;
    public short        bottom;
    public byte         displayAt;
    public fixed byte   unknown2[4];
    public bool         isContinuous;
    public bool         isFullScreen;
    public fixed byte   unknown3[2];
    public int          registryId;
    public byte         unknown4;
    public uint         extractStart;
    public uint         extractStop;
    public byte         panel;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 28)]
  public unsafe struct InfComponentsShape
  {
    public byte         unknown1;
    public short        left;
    public short        top;
    public short        right;
    public short        bottom;
    public byte         displayAt;
    public fixed byte   unknown2[18];
  }
}
