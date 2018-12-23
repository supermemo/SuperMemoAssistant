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
// Created On:   2018/06/12 21:05
// Modified On:  2018/12/20 04:01
// Modified By:  Alexis

#endregion




using System;
using Process.NET;
using Process.NET.Assembly;
using Process.NET.Assembly.Assemblers;
using Process.NET.Native.Types;
using Process.NET.Patterns;
using Process.NET.Threads;
using SuperMemoAssistant.Extensions;

namespace SuperMemoAssistant.Sys
{
  public delegate TRet NativeFunc<out TRet>(IRemoteThread           executingThread                                                                   = null);
  public delegate TRet NativeFunc<out TRet, in T1>(T1               param1, IRemoteThread executingThread                                             = null);
  public delegate TRet NativeFunc<out TRet, in T1, in T2>(T1        param1, T2            param2, IRemoteThread executingThread                       = null);
  public delegate TRet NativeFunc<out TRet, in T1, in T2, in T3>(T1 param1, T2            param2, T3            param3, IRemoteThread executingThread = null);
  public delegate bool NativeAction(IRemoteThread                   executingThread                                                                   = null);
  public delegate bool NativeAction<in T1>(T1                       param1, IRemoteThread executingThread                                             = null);
  public delegate bool NativeAction<in T1, in T2>(T1                param1, T2            param2, IRemoteThread executingThread                       = null);
  public delegate bool NativeAction<in T1, in T2, in T3>(T1         param1, T2            param2, T3            param3, IRemoteThread executingThread = null);


  public class NativeFuncScanner : IDisposable
  {
    #region Properties & Fields - Non-Public

    protected PatternScanner  Scanner { get; set; }
    protected AssemblyFactory Factory { get; set; }
    protected IProcess        Process { get; }

    #endregion




    #region Constructors

    public NativeFuncScanner(IProcess           process,
                             CallingConventions convention)
    {
      Process    = process;
      Convention = convention;
      Scanner    = new PatternScanner(Process.ModuleFactory.MainModule);
      Factory = new AssemblyFactory(process,
                                    new Fasm32Assembler());
    }


    /// <inheritdoc />
    public void Dispose()
    {
      Factory?.Dispose();

      Factory = null;
      Scanner = null;
    }

    #endregion




    #region Properties & Fields - Public

    public CallingConventions Convention { get; }

    #endregion




    #region Methods

    public NativeAction GetNativeAction(IMemoryPattern pattern)
    {
      var scanRes = Scanner.Find(pattern);

      if (scanRes.Found == false)
        return null;

      return (t) => new Action(() => Execute(scanRes.BaseAddress,
                                             t)).ExceptionToDefault();
    }

    public NativeAction<T1> GetNativeAction<T1>(IMemoryPattern pattern)
    {
      var scanRes = Scanner.Find(pattern);

      if (scanRes.Found == false)
        return null;

      return (p1,
              t) => new Action(() => Execute(scanRes.BaseAddress,
                                             t,
                                             p1)).ExceptionToDefault();
    }

    public NativeAction<T1, T2> GetNativeAction<T1, T2>(IMemoryPattern pattern)
    {
      var scanRes = Scanner.Find(pattern);

      if (scanRes.Found == false)
        return null;

      return (p1,
              p2,
              t) => new Action(() => Execute(scanRes.BaseAddress,
                                             t,
                                             p1,
                                             p2)).ExceptionToDefault();
    }

    public NativeAction<T1, T2, T3> GetNativeAction<T1, T2, T3>(IMemoryPattern pattern)
    {
      var scanRes = Scanner.Find(pattern);

      if (scanRes.Found == false)
        return null;

      return (p1,
              p2,
              p3,
              t) => new Action(() => Execute(scanRes.BaseAddress,
                                             t,
                                             p1,
                                             p2,
                                             p3)).ExceptionToDefault();
    }

    public NativeFunc<TRet> GetNativeFunc<TRet>(IMemoryPattern pattern)
    {
      var scanRes = Scanner.Find(pattern);

      if (scanRes.Found == false)
        return null;

      return (t) => new Func<TRet>(() => Execute<TRet>(scanRes.BaseAddress,
                                                       t)).ExceptionToDefault();
    }

    public NativeFunc<TRet, T1> GetNativeFunc<TRet, T1>(IMemoryPattern pattern)
    {
      var scanRes = Scanner.Find(pattern);

      if (scanRes.Found == false)
        return null;

      return (p1,
              t) => new Func<TRet>(() => Execute<TRet>(scanRes.BaseAddress,
                                                       t,
                                                       p1)).ExceptionToDefault();
    }

    public NativeFunc<TRet, T1, T2> GetNativeFunc<TRet, T1, T2>(IMemoryPattern pattern)
    {
      var scanRes = Scanner.Find(pattern);

      if (scanRes.Found == false)
        return null;

      return (p1,
              p2,
              t) => new Func<TRet>(() => Execute<TRet>(scanRes.BaseAddress,
                                                       t,
                                                       p1,
                                                       p2)).ExceptionToDefault();
    }

    public NativeFunc<TRet, T1, T2, T3> GetNativeFunc<TRet, T1, T2, T3>(IMemoryPattern pattern)
    {
      var scanRes = Scanner.Find(pattern);

      if (scanRes.Found == false)
        return null;

      return (p1,
              p2,
              p3,
              t) => new Func<TRet>(() => Execute<TRet>(scanRes.BaseAddress,
                                                       t,
                                                       p1,
                                                       p2,
                                                       p3)).ExceptionToDefault();
    }


    public void Execute(IntPtr           baseAddr,
                        IRemoteThread    remoteThread,
                        params dynamic[] args)
    {
      Factory.Execute(baseAddr,
                      Convention,
                      remoteThread,
                      args);
    }

    public TRet Execute<TRet>(IntPtr           baseAddr,
                              IRemoteThread    remoteThread,
                              params dynamic[] args)
    {
        return Factory.Execute<TRet>(baseAddr,
                                     Convention,
                                     remoteThread,
                                     args);
    }

    public void Cleanup()
    {
      Scanner = null;
    }

    #endregion
  }
}
