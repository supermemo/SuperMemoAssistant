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
// Modified On:  2019/12/14 20:01
// Modified By:  Alexis

#endregion




using System;
using Process.NET.Execution;
using Process.NET.Memory;
using Process.NET.Native.Types;
using Process.NET.Patterns;
using Process.NET.Types;

// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

namespace SuperMemoAssistant.SuperMemo.SuperMemo17
{
  public partial class SM17Natives
  {
    public class TRegistry17 : TRegistry
    {
      #region Constructors

      public TRegistry17()
      {
        TextRegistryInstance     = new ObjPtr(TDatabase.InstancePtr, 0x4E7);
        ImageRegistryInstance    = new ObjPtr(TDatabase.InstancePtr, 0x4FF);
        SoundRegistryInstance    = new ObjPtr(TDatabase.InstancePtr, 0x503);
        VideoRegistryInstance    = new ObjPtr(TDatabase.InstancePtr, 0x507);
        BinaryRegistryInstance   = new ObjPtr(TDatabase.InstancePtr, 0x513);
        TemplateRegistryInstance = new ObjPtr(TDatabase.InstancePtr, 0x517);
        ConceptRegistryInstance  = new ObjPtr(TDatabase.InstancePtr, 0x527);

        AddMemberCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 89 45 E8 8D 55 EC",
          1);
        ImportFileCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 89 45 D8 8B 4D F4",
          1);

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

      public override ObjPtr TextRegistryInstance     { get; }
      public override ObjPtr ImageRegistryInstance    { get; }
      public override ObjPtr SoundRegistryInstance    { get; }
      public override ObjPtr VideoRegistryInstance    { get; }
      public override ObjPtr BinaryRegistryInstance   { get; }
      public override ObjPtr TemplateRegistryInstance { get; }
      public override ObjPtr ConceptRegistryInstance  { get; }


      // TRegistry.AddMember
      public override IMemoryPattern AddMemberCallSig { get; }
      // TRegistry.ImportFile
      public override IMemoryPattern ImportFileCallSig { get; }

      public override Procedure<Func<IntPtr, DelphiUTF16String, int>>                    AddMember  { get; }
      public override Procedure<Func<IntPtr, DelphiUTF16String, DelphiUTF16String, int>> ImportFile { get; }

      #endregion
    }
  }
}
