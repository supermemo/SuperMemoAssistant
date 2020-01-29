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
// Modified On:  2020/01/28 22:12
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.SMA;
using SuperMemoAssistant.SuperMemo.Common.Registry;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Members
{
  public class Concept : RegistryMemberBase, IConcept
  {
    #region Constructors

    public Concept(int id)
      : base(id) { }

    #endregion




    #region Properties Impl - Public

    public IConceptGroup ConceptGroup => (IConceptGroup)Core.SM.Registry.Element?[SlotLengthOrConceptGroupId];

    public int ElementId => SlotLengthOrConceptGroupId;

    #endregion




    #region Methods Impl

    public override string GetFilePath()
    {
      return null;
    }

    public Task<bool> DeleteAsync()
    {
      throw new NotImplementedException();
    }

    public IEnumerable<IElement> GetLinkedElements()
    {
      throw new NotImplementedException();
    }

    public Task<bool> NeuralAsync()
    {
      throw new NotImplementedException();
    }

    public Task<bool> RenameAsync(string newName)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
