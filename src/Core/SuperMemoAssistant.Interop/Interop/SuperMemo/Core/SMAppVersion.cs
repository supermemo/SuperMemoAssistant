#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   2018/07/27 12:55
// Modified On:  2018/12/13 13:07
// Modified By:  Alexis

#endregion




using System;
using JetBrains.Annotations;

namespace SuperMemoAssistant.Interop.SuperMemo.Core
{
  [Serializable]
  public class SMAppVersion
  {
    #region Constructors

    public SMAppVersion(int major,
                        int minor,
                        int increment = 0)
    {
      Major     = major;
      Minor     = minor;
      Increment = increment;
    }

    #endregion




    #region Properties & Fields - Public

    public int Major     { get; }
    public int Minor     { get; }
    public int Increment { get; }

    #endregion




    #region Methods Impl

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

    #endregion




    #region Methods

    public static bool operator ==([NotNull] SMAppVersion v1,
                                   [NotNull] SMAppVersion v2)
    {
      return v1.Major == v2.Major && v1.Minor == v2.Minor && v1.Increment == v2.Increment;
    }

    public static bool operator !=([NotNull] SMAppVersion v1,
                                   [NotNull] SMAppVersion v2)
    {
      return v1.Major != v2.Major || v1.Minor != v2.Minor || v1.Increment != v2.Increment;
    }

    public static bool operator >([NotNull] SMAppVersion v1,
                                  [NotNull] SMAppVersion v2)
    {
      return
        v1.Major > v2.Major ||
        v1.Major == v2.Major && v1.Minor > v2.Minor ||
        v1.Major == v2.Major && v1.Minor == v2.Minor && v1.Increment > v2.Increment;
    }

    public static bool operator <([NotNull] SMAppVersion v1,
                                  [NotNull] SMAppVersion v2)
    {
      return
        v1.Major < v2.Major ||
        v1.Major == v2.Major && v1.Minor < v2.Minor ||
        v1.Major == v2.Major && v1.Minor == v2.Minor && v1.Increment < v2.Increment;
    }

    public static bool operator >=([NotNull] SMAppVersion v1,
                                   [NotNull] SMAppVersion v2)
    {
      return v1 == v2 || v1 > v2;
    }

    public static bool operator <=([NotNull] SMAppVersion v1,
                                   [NotNull] SMAppVersion v2)
    {
      return v1 == v2 || v1 < v2;
    }

    #endregion
  }
}
