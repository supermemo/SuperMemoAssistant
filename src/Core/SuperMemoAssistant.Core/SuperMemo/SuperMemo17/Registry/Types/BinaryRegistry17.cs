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
// Created On:   2020/03/29 00:20
// Modified On:  2020/04/10 14:20
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Types
{
  using System;
  using Common.Registry;
  using Common.Registry.Files;
  using Interop.SuperMemo.Registry.Members;
  using Interop.SuperMemo.Registry.Types;
  using Members;
  using SMA;

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "<Pending>")]
  public class BinaryRegistry17 : RegistryBase<Binary, IBinary>, IBinaryRegistry
  {
    #region Properties & Fields - Non-Public

    /// <inheritdoc />
    protected override IRegistryFileDescriptor FileDesc { get; } = new BinaryFileDescriptor();
    /// <inheritdoc />
    protected override IRegistryUpdater Updater { get; }

    protected override IntPtr RegistryPtr =>
      new IntPtr(Core.Natives.Registry.BinaryRegistryInstance.Read<int>(Core.SMA.SMProcess.Memory));

    #endregion




    #region Constructors

    /// <inheritdoc />
    public BinaryRegistry17()
    {
      Updater = new RegistryUpdater17<Binary, IBinary>(this, OnMemberAddedOrUpdated);
    }

    #endregion




    #region Methods Impl

    public override Binary CreateInternal(int id)
    {
      return new Binary(id);
    }

    public int Add(string filePath, string registryName)
    {
      return ImportFile(filePath, registryName);
    }

    #endregion
  }
}
