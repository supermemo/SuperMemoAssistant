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
// Created On:   2019/08/09 18:26
// Modified On:  2022/12/17 20:01
// Modified By:  - Alexis
//               - Ki

#endregion




using System;
using Process.NET.Execution;
using Process.NET.Memory;
using Process.NET.Native.Types;
using Process.NET.Patterns;
using Process.NET.Types;
using SuperMemoAssistant.Extensions;

// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

namespace SuperMemoAssistant.SuperMemo.Natives
{
  public partial class SMNatives
  {
    public class TRegistry
    {
      #region Constructors

      public TRegistry(TDatabase db, NativeData nativeData)
      {
        TextRegistryInstance     = new ObjPtr(db.InstancePtr, nativeData.Pointers[NativePointer.Registry_TextRegistryInstance]);
        CommentRegistryInstance  = new ObjPtr(db.InstancePtr, nativeData.Pointers[NativePointer.Registry_CommentRegistryInstance]);
        ImageRegistryInstance    = new ObjPtr(db.InstancePtr, nativeData.Pointers[NativePointer.Registry_ImageRegistryInstance]);
        SoundRegistryInstance    = new ObjPtr(db.InstancePtr, nativeData.Pointers[NativePointer.Registry_SoundRegistryInstance]);
        VideoRegistryInstance    = new ObjPtr(db.InstancePtr, nativeData.Pointers[NativePointer.Registry_VideoRegistryInstance]);
        BinaryRegistryInstance   = new ObjPtr(db.InstancePtr, nativeData.Pointers[NativePointer.Registry_BinaryRegistryInstance]);
        TemplateRegistryInstance = new ObjPtr(db.InstancePtr, nativeData.Pointers[NativePointer.Registry_TemplateRegistryInstance]);
        ConceptRegistryInstance  = new ObjPtr(db.InstancePtr, nativeData.Pointers[NativePointer.Registry_ConceptRegistryInstance]);

        AddMemberCallSig = nativeData.GetMemoryPattern(NativeMethod.TRegistry_AddMember);
        ImportFileCallSig = nativeData.GetMemoryPattern(NativeMethod.TRegistry_ImportFile);

        AddMember = new Procedure<Func<IntPtr, DelphiUTF16String, int>>(
          "AddMember",
          CallingConventions.Register,
          AddMemberCallSig
        );
        ImportFile = new Procedure<Func<IntPtr, DelphiUTF16String, DelphiUTF16String, int>>(
          "ImportFile",
          CallingConventions.Register,
          ImportFileCallSig
        );
      }

      #endregion




      #region Properties Impl - Public

      public ObjPtr TextRegistryInstance     { get; }
      public ObjPtr CommentRegistryInstance  { get; }
      public ObjPtr ImageRegistryInstance    { get; }
      public ObjPtr SoundRegistryInstance    { get; }
      public ObjPtr VideoRegistryInstance    { get; }
      public ObjPtr BinaryRegistryInstance   { get; }
      public ObjPtr TemplateRegistryInstance { get; }
      public ObjPtr ConceptRegistryInstance  { get; }


      // TRegistry.AddMember
      public IMemoryPattern AddMemberCallSig { get; }
      // TRegistry.ImportFile
      public IMemoryPattern ImportFileCallSig { get; }

      public Procedure<Func<IntPtr, DelphiUTF16String, int>>                    AddMember  { get; }
      public Procedure<Func<IntPtr, DelphiUTF16String, DelphiUTF16String, int>> ImportFile { get; }

      #endregion
    }
  }
}
