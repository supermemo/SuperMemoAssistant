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
// Created On:   2018/05/18 18:28
// Modified On:  2018/11/19 16:43
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Members
{
  public class Text : RegistryMemberBase, IText
  {
    #region Constructors

    public Text(int        id,
                RegMemElem mem,
                RegRtElem  rt)
      : base(id,
             mem,
             rt) { }

    #endregion




    #region Properties Impl - Public

    public string Name => RtxValue;

    public string Value
    {
      get
      {
        switch (LinkType)
        {
          case RegistryLinkType.Deleted:
            return null;

          case RegistryLinkType.Rtx:
            return RtxValue;

          case RegistryLinkType.FileAndRtx:
            return File.ReadAllText(GetFilePath());

          case RegistryLinkType.Rtf:
            break;
        }

        throw new NotImplementedException();
      }
      set
      {
        switch (LinkType)
        {
          case RegistryLinkType.Deleted:
            throw new ArgumentException($"Text member {Id} is deleted.");

          case RegistryLinkType.Rtx:
            break;

          case RegistryLinkType.FileAndRtx:
            File.WriteAllText(GetFilePath(),
                              value);
            return;

          case RegistryLinkType.Rtf:
            break;
        }

        throw new NotImplementedException();
      }
    }

    #endregion




    #region Methods Impl

    public string GetFilePath()
    {
      return GetFilePath("htm");
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
