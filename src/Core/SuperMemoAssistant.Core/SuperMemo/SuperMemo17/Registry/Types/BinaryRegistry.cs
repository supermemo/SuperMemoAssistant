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
// Created On:   2018/12/07 13:56
// Modified On:  2018/12/10 13:12
// Modified By:  Alexis

#endregion




using System;
using SuperMemoAssistant.Hooks.SuperMemo;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Types;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Members;
using SuperMemoAssistant.Sys;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Types
{
  [InitOnLoad]
  public class BinaryRegistry : RegistryBase<Binary, IBinary>, IBinaryRegistry
  {
    #region Constants & Statics

    public static BinaryRegistry Instance { get; } = new BinaryRegistry();

    #endregion




    #region Properties & Fields - Non-Public

    protected override bool IsOptional => true;

    protected override string MemFileName => SMConst.Files.BinaryMemFileName;
    protected override string RtxFileName => SMConst.Files.BinaryRtxFileName;
    protected override string RtfFileName => null;
    protected override IntPtr RegistryPtr =>
      new IntPtr(SMNatives.TRegistry.BinaryRegistryInstance.Read<int>(SMA.Instance.SMProcess.Memory));

    #endregion




    #region Constructors

    protected BinaryRegistry() { }

    #endregion




    #region Methods Impl

    protected override Binary Create(int        id,
                                     RegMemElem mem,
                                     RegRtElem  rtxOrRtf)
    {
      return new Binary(id,
                        mem,
                        rtxOrRtf);
    }

    public int AddMember(string filePath,
                         string registryName)
    {
      return ImportFile(filePath,
                        registryName);
    }

    #endregion
  }
}
