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
// Created On:   2018/05/19 12:02
// Modified On:  2018/05/30 23:26
// Modified By:  Alexis

#endregion




using System;
using System.Threading.Tasks;
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
  public class ConceptRegistry : RegistryBase<Concept, IConcept>, IConceptRegistry
  {
    #region Constants & Statics

    public static ConceptRegistry Instance { get; } = new ConceptRegistry();

    #endregion




    #region Properties & Fields - Non-Public

    protected override string MemFileName => SMConst.Files.ConceptMemFileName;
    protected override string RtxFileName => SMConst.Files.ConceptRtxFileName;
    protected override string RtfFileName => null;
    protected override IntPtr RegistryPtr => new IntPtr(SMNatives.TRegistry.ConceptRegistryInstance.Read<int>(SMA.Instance.SMProcess.Memory));

    #endregion




    #region Constructors

    protected ConceptRegistry() { }

    #endregion




    #region Methods Impl

    protected override Concept Create(int id, RegMemElem mem, RegRtElem rtxOrRtf)
    {
      return new Concept(id, mem, rtxOrRtf);
    }

    public Task<IConcept> Add(string text)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
