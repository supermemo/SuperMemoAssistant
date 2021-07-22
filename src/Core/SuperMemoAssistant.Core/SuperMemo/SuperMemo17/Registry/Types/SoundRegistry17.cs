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
// Modified On:  2020/04/09 14:54
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Types
{
  using System;
  using Anotar.Serilog;
  using Common.Registry;
  using Common.Registry.Files;
  using Interop.SuperMemo.Registry.Members;
  using Interop.SuperMemo.Registry.Types;
  using Members;
  using SMA;

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "<Pending>")]
  public class SoundRegistry17 : RegistryBase<Sound, ISound>, ISoundRegistry
  {
    #region Properties & Fields - Non-Public

    /// <inheritdoc />
    protected override IRegistryFileDescriptor FileDesc { get; } = new SoundFileDescriptor();
    /// <inheritdoc />
    protected override IRegistryUpdater Updater { get; }

    protected override IntPtr RegistryPtr =>
      new IntPtr(Core.Natives.Registry.SoundRegistryInstance.Read<int>(Core.SMA.SMProcess.Memory));

    #endregion




    #region Constructors

    /// <inheritdoc />
    public SoundRegistry17()
    {
      Updater = new RegistryUpdater17<Sound, ISound>(this, OnMemberAddedOrUpdated);
    }

    #endregion




    #region Methods Impl
    
    /// <inheritdoc />
    public override Sound CreateInternal(int id)
    {
      return Members[id] = new Sound(id);
    }

    /// <inheritdoc />
    public int Add(string filePath, string registryName)
    {
      try
      {
        var ret = ImportFile(filePath, registryName);

        return ret;
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Failed to add sound to registry");
        return -1;
      }
    }

    #endregion
  }
}
