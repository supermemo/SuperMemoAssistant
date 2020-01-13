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
// Created On:   2019/08/07 14:44
// Modified On:  2020/01/12 10:30
// Modified By:  Alexis

#endregion




using System;
using System.IO;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Types;
using SuperMemoAssistant.SMA;
using SuperMemoAssistant.SuperMemo.Common.Registry;
using SuperMemoAssistant.SuperMemo.Common.Registry.Files;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Members;
using SuperMemoAssistant.Sys.Drawing;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Types
{
  public class ImageRegistry17 : RegistryBase<Image, IImage>, IImageRegistry
  {
    #region Properties & Fields - Non-Public

    /// <inheritdoc />
    protected override IRegistryFileDescriptor FileDesc { get; } = new ImageFileDescriptor();
    /// <inheritdoc />
    protected override IRegistryUpdater Updater { get; }

    protected override IntPtr RegistryPtr =>
      new IntPtr(Core.Natives.Registry.ImageRegistryInstance.Read<int>(Core.SMA.SMProcess.Memory));

    #endregion




    #region Constructors

    /// <inheritdoc />
    public ImageRegistry17()
    {
      Updater = new RegistryUpdater17<Image, IImage>(this, OnMemberAddedOrUpdated);
    }

    #endregion




    #region Methods Impl

    public override Image CreateInternal(int id)
    {
      return Members[id] = new Image(id);
    }

    public Task<IImage> AddAsync(string imageName,
                                 string imagePath)
    {
      throw new NotImplementedException();
    }

    public int AddMember(ImageWrapper imageWrapper,
                         string       registryName)
    {
      try
      {
        var      filePath = Path.GetTempFileName() + ".png";
        FileInfo fi       = imageWrapper.ToFile(filePath);

        var ret = ImportFile(filePath,
                             registryName);

        fi.Delete();

        return ret;
      }
      catch
      {
        return -1;
      }
    }

    #endregion
  }
}
