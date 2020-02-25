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
// Created On:   2018/06/07 01:50
// Modified On:  2019/01/18 20:48
// Modified By:  Alexis

#endregion




using System;
using PluginManager.Interop.Sys;

namespace SuperMemoAssistant.Sys.Remoting
{
  public class ActionProxy : PerpetualMarshalByRefObject
  {
    #region Properties & Fields - Non-Public

    private readonly Action _action;

    #endregion




    #region Constructors

    public ActionProxy(Action action)
    {
      _action = action;
    }

    #endregion




    #region Methods

    public void Invoke()
    {
      _action();
    }

    public static implicit operator Action(ActionProxy aProxy)
    {
      return aProxy.Invoke;
    }

    public static implicit operator ActionProxy(Action action)
    {
      return new ActionProxy(action);
    }

    #endregion
  }

  public class ActionProxy<T1> : PerpetualMarshalByRefObject
  {
    #region Properties & Fields - Non-Public

    private readonly Action<T1> _action;

    #endregion




    #region Constructors

    public ActionProxy(Action<T1> action)
    {
      _action = action;
    }

    #endregion




    #region Methods

    public void Invoke(T1 arg1)
    {
      _action(arg1);
    }

    public static implicit operator Action<T1>(ActionProxy<T1> aProxy)
    {
      return aProxy.Invoke;
    }

    public static implicit operator ActionProxy<T1>(Action<T1> action)
    {
      return new ActionProxy<T1>(action);
    }

    #endregion
  }

  public class ActionProxy<T1, T2> : PerpetualMarshalByRefObject
  {
    #region Properties & Fields - Non-Public

    private readonly Action<T1, T2> _action;

    #endregion




    #region Constructors

    public ActionProxy(Action<T1, T2> action)
    {
      _action = action;
    }

    #endregion




    #region Methods

    public void Invoke(T1 arg1, T2 arg2)
    {
      _action(arg1, arg2);
    }

    public static implicit operator Action<T1, T2>(ActionProxy<T1, T2> aProxy)
    {
      return aProxy.Invoke;
    }

    public static implicit operator ActionProxy<T1, T2>(Action<T1, T2> action)
    {
      return new ActionProxy<T1, T2>(action);
    }

    #endregion
  }
}
