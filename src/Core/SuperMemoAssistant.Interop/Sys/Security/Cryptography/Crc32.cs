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
// Created On:   2018/06/03 03:33
// Modified On:  2019/01/20 08:32
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SuperMemoAssistant.Sys.Security.Cryptography
{
  /// <summary>Implements a 32-bit CRC hash algorithm compatible with Zip etc.</summary>
  /// <remarks>
  ///   Crc32 should only be used for backward compatibility with older file formats and
  ///   algorithms. It is not secure enough for new applications. If you need to call multiple times
  ///   for the same data either use the HashAlgorithm interface or remember that the result of one
  ///   Compute call needs to be ~ (XOR) before being passed in as the seed for the next Compute
  ///   call.
  /// </remarks>
  public sealed class Crc32 : HashAlgorithm
  {
    #region Constants & Statics

    public const UInt32 DefaultPolynomial = 0xedb88320u;
    public const UInt32 DefaultSeed       = 0xffffffffu;

    private static UInt32[] _defaultTable;

    #endregion




    #region Properties & Fields - Non-Public

    private readonly UInt32   _seed;
    private readonly UInt32[] _table;
    private          UInt32   _hash;

    #endregion




    #region Constructors

    public Crc32()
      : this(DefaultPolynomial, DefaultSeed) { }

    public Crc32(UInt32 polynomial,
                 UInt32 seed)
    {
      _table = InitializeTable(polynomial);
      _seed  = _hash = seed;
    }

    #endregion




    #region Properties Impl - Public

    public override int HashSize { get { return 32; } }

    #endregion




    #region Methods Impl

    public override void Initialize()
    {
      _hash = _seed;
    }

    protected override void HashCore(byte[] array,
                                     int    ibStart,
                                     int    cbSize)
    {
      _hash = CalculateHash(_table, _hash, array, ibStart, cbSize);
    }

    protected override byte[] HashFinal()
    {
      var hashBuffer = UInt32ToBigEndianBytes(~_hash);
      HashValue = hashBuffer;
      return hashBuffer;
    }

    #endregion




    #region Methods

    public static UInt32 Compute(byte[] buffer)
    {
      return Compute(DefaultSeed, buffer);
    }

    public static UInt32 Compute(UInt32 seed,
                                 byte[] buffer)
    {
      return Compute(DefaultPolynomial, seed, buffer);
    }

    public static UInt32 Compute(UInt32 polynomial,
                                 UInt32 seed,
                                 byte[] buffer)
    {
      return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
    }

    private static UInt32[] InitializeTable(UInt32 polynomial)
    {
      if (polynomial == DefaultPolynomial && _defaultTable != null)
        return _defaultTable;

      var createTable = new UInt32[256];
      for (var i = 0; i < 256; i++)
      {
        var entry = (UInt32)i;
        for (var j = 0; j < 8; j++)
          if ((entry & 1) == 1)
            entry = (entry >> 1) ^ polynomial;
          else
            entry = entry >> 1;
        createTable[i] = entry;
      }

      if (polynomial == DefaultPolynomial)
        _defaultTable = createTable;

      return createTable;
    }

    private static UInt32 CalculateHash(UInt32[]    table,
                                        UInt32      seed,
                                        IList<byte> buffer,
                                        int         start,
                                        int         size)
    {
      var hash = seed;
      for (var i = start; i < start + size; i++)
        hash = (hash >> 8) ^ table[buffer[i] ^ (hash & 0xff)];
      return hash;
    }

    private static byte[] UInt32ToBigEndianBytes(UInt32 uint32)
    {
      var result = BitConverter.GetBytes(uint32);

      if (BitConverter.IsLittleEndian)
        Array.Reverse(result);

      return result;
    }

    #endregion
  }
}
