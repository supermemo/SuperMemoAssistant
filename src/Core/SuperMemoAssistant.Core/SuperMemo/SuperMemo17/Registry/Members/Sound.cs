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

#endregion




namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Members
{
  using System;
  using System.Collections.Generic;
  using Common.Registry;
  using Interop.SuperMemo.Elements.Types;
  using Interop.SuperMemo.Registry.Members;
  
  /// <inheritdoc cref="ISound" />
  public class Sound : RegistryMemberBase, ISound
  {
    #region Constructors

    public Sound(int id)
      : base(id) { }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public bool Delete()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc cref="IRegistryMember" />
    public override string GetFilePath()
    {
      //TODO: switch () on filetype
      return TryFilePathOrSearch("mp3");
    }


    public IEnumerable<IElement> GetLinkedElements()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public bool Neural()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public bool Rename(string newName)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
