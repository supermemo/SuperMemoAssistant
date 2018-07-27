using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static SuperMemoAssistant.SuperMemo.System.Collections.SparseClusteredArray<int>;

namespace SuperMemoAssistant.SuperMemo.System.Collections
{
  public class SparseClusteredArrayTests
  {
    Random Random = new Random();

    [Fact]
    public void OverlappingTest()
    {
      int length = 100;
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

        int lower = Random.Next(98);
        int upper = lower + Random.Next(99 - lower) + 1;
        Bounds bounds = new Bounds(lower, upper);
        var arr = GenerateBytes(bounds, validationArray);

        sca.AddOrUpdate(arr, bounds.Lower);
        Validate(sca, validationArray, -1);
      }
    }

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

    private int[] GenerateBytes(IBounds bounds, int[] validationArr)
    {
      int length = bounds.Length();
      int[] ret = new int[length];

      for (int i = bounds.Lower; i <= bounds.Upper; i++)
        validationArr[i] = ret[i - bounds.Lower] = Random.Next();

      return ret;
    }
  }
}
