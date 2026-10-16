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
  using System.IO;
  using Anotar.Serilog;
  using Common.Registry;
  using Common.Registry.Models;
  using Interop.SuperMemo.Elements.Types;
  using Interop.SuperMemo.Registry.Members;

  /// <inheritdoc cref="IText" />
  public class Comment : RegistryMemberBase, IComment
  {
    #region Constructors

    public Comment(int id)
      : base(id) { }

    #endregion




    #region Properties Impl - Public

#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
    /// <inheritdoc />
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
            throw new NotImplementedException("RTF text is not supported yet.");
        }

        LogTo.Error("Getting unknown LinkType {LinkType}", LinkType);
        throw new InvalidOperationException($"Setting unknown LinkType {LinkType}");
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
            File.WriteAllText(GetFilePath(), value);
            return;

          case RegistryLinkType.Rtf:
            throw new NotImplementedException("RTF text is not supported yet.");
        }

        LogTo.Error("Setting unknown LinkType {LinkType}", LinkType);
        throw new InvalidOperationException($"Setting unknown LinkType {LinkType}");
      }
    }
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations

    #endregion




    #region Methods Impl

    /// <inheritdoc cref="IRegistryMember" />
    public override string GetFilePath()
    {
      return TryFilePathOrSearch("htm");
    }

    /// <inheritdoc />
    public bool Delete()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
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
