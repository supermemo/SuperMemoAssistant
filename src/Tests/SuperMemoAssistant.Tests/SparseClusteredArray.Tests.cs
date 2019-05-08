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
// Created On:   2019/03/02 18:29
// Modified On:  2019/04/24 02:31
// Modified By:  Alexis

#endregion




using System;
using SuperMemoAssistant.Sys.SparseClusteredArray;
using Xunit;

namespace SuperMemoAssistant.Tests
{
  public class SparseClusteredArrayTests
  {
    private readonly Random _random = new Random();

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    private void Validate<T>(SparseClusteredArray<T> sca, T[] vArr, T defaultVal)
    {
      int idxVal = 0;

      foreach (var seg in sca)
      {
        for (; idxVal < seg._absIdx; idxVal++)
          Assert.Equal(vArr[idxVal], defaultVal);

        int fromIdx = seg._fromIdx;

        for (; fromIdx <= seg._toIdx; fromIdx++, idxVal++)
          Assert.Equal(seg._arr[fromIdx], vArr[idxVal]);
      }

      for (; idxVal < vArr.Length; idxVal++)
        Assert.Equal(vArr[idxVal], defaultVal);
    }

    private T[] GenerateValidationArray<T>(int length, T defaultVal)
    {
      T[] validationArray = new T[length];

      for (int i = 0; i < length; i++)
        validationArray[i] = defaultVal;

      return validationArray;
    }

    private int[] GenerateBytes<T>(SparseClusteredArray<T>.IBounds bounds, int[] validationArr)
    {
      int   length = bounds.Length();
      int[] ret    = new int[length];

      for (int i = bounds.Lower; i <= bounds.Upper; i++)
        validationArr[i] = ret[i - bounds.Lower] = _random.Next();

      return ret;
    }

    [Fact]
    public void OverlappingTest()
    {
      int   length          = 100;
      int[] validationArray = GenerateValidationArray(length, -1);

      SparseClusteredArray<int> sca = new SparseClusteredArray<int>();
      /*
      List<IBounds> boundsArr = new List<IBounds>()
      {
        new Bounds(1, 25),
        new Bounds(26, 30),
        new Bounds(35, 50),
        new Bounds(1, 25),
        new Bounds(5, 25),
        new Bounds(1, 20),
        new Bounds(11, 27),
        new Bounds(32, 70),
        new Bounds(1, 25),
        new Bounds(11, 27),
        new Bounds(1, 25),
        new Bounds(5, 25),
        new Bounds(26, 30),
        new Bounds(35, 50),
        new Bounds(1, 20),
        new Bounds(32, 70),
        new Bounds(2, 65),
        new Bounds(0, 99),
      };

      foreach (var bounds in boundsArr)
      {
        var arr = GenerateBytes(bounds, validationArray);

        sca.AddOrUpdate(arr, bounds.Lower);
        Validate(sca, validationArray, -1);
      }*/


      for (int i = 0; i < 500000; i++)
      {
        if (i % 10 == 0)
        {
          sca = new SparseClusteredArray<int>();

          validationArray = GenerateValidationArray(length, -1);
        }

        int                           lower  = _random.Next(98);
        int                           upper  = lower + _random.Next(99 - lower) + 1;
        SparseClusteredArray<int>.Bounds bounds = new SparseClusteredArray<int>.Bounds(lower, upper);
        var                           arr    = GenerateBytes(bounds, validationArray);

        sca.Write(arr, bounds.Lower);
        Validate(sca, validationArray, -1);
      }
    }
  }
}
