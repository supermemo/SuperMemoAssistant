using System;

namespace SuperMemoAssistant.Interop.SuperMemo.Core
{
  [Serializable]
  public class SMAppVersion
  {
    public SMAppVersion(int major, int minor, int increment = 0)
    {
      Major = major;
      Minor = minor;
      Increment = increment;
    }

    public int Major { get; set; }
    public int Minor { get; set; }
    public int Increment { get; set; }

    public static bool operator ==(SMAppVersion v1, SMAppVersion v2)
    {
      return v1.Major == v2.Major && v1.Minor == v2.Minor && v1.Increment == v2.Increment;
    }

    public static bool operator !=(SMAppVersion v1, SMAppVersion v2)
    {
      return v1.Major != v2.Major || v1.Minor != v2.Minor || v1.Increment != v2.Increment;
    }

    public static bool operator >(SMAppVersion v1, SMAppVersion v2)
    {
      return
        v1.Major > v2.Major ||
        (v1.Major == v2.Major && v1.Minor > v2.Minor) ||
        (v1.Major == v2.Major && v1.Minor == v2.Minor && v1.Increment > v2.Increment);
    }

    public static bool operator <(SMAppVersion v1, SMAppVersion v2)
    {
      return
        v1.Major < v2.Major ||
        (v1.Major == v2.Major && v1.Minor < v2.Minor) ||
        (v1.Major == v2.Major && v1.Minor == v2.Minor && v1.Increment < v2.Increment);
    }

    public static bool operator >=(SMAppVersion v1, SMAppVersion v2)
    {
      return v1 == v2 || v1 > v2;
    }

    public static bool operator <=(SMAppVersion v1, SMAppVersion v2)
    {
      return v1 == v2 || v1 < v2;
    }

    public override bool Equals(object obj)
    {
      var version = obj as SMAppVersion;
      return version != null &&
             this == version;
    }

    public override int GetHashCode()
    {
      var hashCode = 314594558;
      hashCode = hashCode * -1521134295 + Major.GetHashCode();
      hashCode = hashCode * -1521134295 + Minor.GetHashCode();
      hashCode = hashCode * -1521134295 + Increment.GetHashCode();
      return hashCode;
    }
  }
}
