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
// Created On:   2018/06/21 11:37
// Modified On:  2018/06/21 11:37
// Modified By:  Alexis
#endregion




using System;
using Anotar.Serilog;

namespace SuperMemoAssistant.Extensions
{
  public static class DelegateEx
  {
    public static bool ExceptionToDefault(this Action action)
    {
      try
      {
        action();

        return true;
      }
      catch (InvalidOperationException ex)
      {
        LogTo.Error(ex,
                    "Call failed");

        return false;
      }
    }
    public static bool ExceptionToDefault<TParam1>(this Action<TParam1> action, TParam1 p1)
    {
      try
      {
        action(p1);

        return true;
      }
      catch (InvalidOperationException ex)
      {
        LogTo.Error(ex,
                    "Call failed");

        return false;
      }
    }
    public static bool ExceptionToDefault<TParam1, TParam2>(this Action<TParam1, TParam2> action, TParam1 p1, TParam2 p2)
    {
      try
      {
        action(p1, p2);

        return true;
      }
      catch (InvalidOperationException ex)
      {
        LogTo.Error(ex,
                    "Call failed");

        return false;
      }
    }
    public static bool ExceptionToDefault<TParam1, TParam2, TParam3>(this Action<TParam1, TParam2, TParam3> action, TParam1 p1, TParam2 p2, TParam3 p3)
    {
      try
      {
        action(p1, p2, p3);

        return true;
      }
      catch (InvalidOperationException ex)
      {
        LogTo.Error(ex,
                    "Call failed");

        return false;
      }
    }

    public static TRet ExceptionToDefault<TRet>(this Func<TRet> func)
    {
      try
      {
        return func();
      }
      catch (InvalidOperationException ex)
      {
        LogTo.Error(ex,
                    "Call failed");

        return default;
      }
    }

    public static TRet ExceptionToDefault<TParam1, TRet>(this Func<TParam1, TRet> func, TParam1 p1)
    {
      try
      {
        return func(p1);
      }
      catch (InvalidOperationException ex)
      {
        LogTo.Error(ex,
                    "Call failed");

        return default;
      }
    }

    public static TRet ExceptionToDefault<TParam1, TParam2, TRet>(this Func<TParam1, TParam2, TRet> func, TParam1 p1, TParam2 p2)
    {
      try
      {
        return func(p1, p2);
      }
      catch (InvalidOperationException ex)
      {
        LogTo.Error(ex,
                    "Call failed");

        return default;
      }
    }

    public static TRet ExceptionToDefault<TParam1, TParam2, TParam3, TRet>(this Func<TParam1, TParam2, TParam3, TRet> func, TParam1 p1, TParam2 p2, TParam3 p3)
    {
      try
      {
        return func(p1, p2, p3);
      }
      catch (InvalidOperationException ex)
      {
        LogTo.Error(ex,
                    "Call failed");

        return default;
      }
    }
  }
}
