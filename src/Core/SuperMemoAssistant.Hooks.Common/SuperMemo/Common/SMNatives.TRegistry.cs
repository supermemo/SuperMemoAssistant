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
// Created On:   2019/12/13 20:04
// Modified On:  2019/12/14 17:17
// Modified By:  Alexis

#endregion




using System;
using Process.NET.Execution;
using Process.NET.Memory;
using Process.NET.Patterns;
using Process.NET.Types;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.SuperMemo.Common
{
  public abstract partial class SuperMemoNatives
  {
    public abstract partial class TRegistry
    {
      #region Properties & Fields - Non-Public

      public abstract Procedure<Func<IntPtr, DelphiUTF16String, int>>                    AddMember                { get; }
      public abstract IMemoryPattern                                                     AddMemberCallSig         { get; }
      public abstract ObjPtr                                                             BinaryRegistryInstance   { get; }
      public abstract ObjPtr                                                             ConceptRegistryInstance  { get; }
      public abstract ObjPtr                                                             ImageRegistryInstance    { get; }
      public abstract Procedure<Func<IntPtr, DelphiUTF16String, DelphiUTF16String, int>> ImportFile               { get; }
      public abstract IMemoryPattern                                                     ImportFileCallSig        { get; }
      public abstract ObjPtr                                                             SoundRegistryInstance    { get; }
      public abstract ObjPtr                                                             TemplateRegistryInstance { get; }
      public abstract ObjPtr                                                             TextRegistryInstance     { get; }
      public abstract ObjPtr                                                             VideoRegistryInstance    { get; }

      #endregion
    }
  }
}
