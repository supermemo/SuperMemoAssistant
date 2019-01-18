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

namespace SuperMemoAssistant.Sys
{
  public class ActionProxy<T> : SMMarshalByRefObject
  {
    #region Properties & Fields - Non-Public

    private readonly Action<T> _action;

    #endregion




    #region Constructors

    public ActionProxy(Action<T> action)
    {
      _action = action;
    }

    #endregion




    #region Methods

    public void Invoke(T args)
    {
      _action(args);
    }

    public static implicit operator Action<T>(ActionProxy<T> aProxy)
    {
      return aProxy.Invoke;
    }

    #endregion
  }
}
