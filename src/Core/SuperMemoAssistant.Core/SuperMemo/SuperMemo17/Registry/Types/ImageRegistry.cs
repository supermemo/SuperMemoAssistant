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
// Created On:   2018/06/01 14:13
// Modified On:  2018/11/26 00:14
// Modified By:  Alexis

#endregion




using System;
using System.IO;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Types;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Files;
using SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Members;
using SuperMemoAssistant.Sys;
using SuperMemoAssistant.Sys.Drawing;

namespace SuperMemoAssistant.SuperMemo.SuperMemo17.Registry.Types
{
  [InitOnLoad]
  public class ImageRegistry : RegistryBase<Image, IImage>, IImageRegistry
  {
    #region Constants & Statics

    public static ImageRegistry Instance { get; } = new ImageRegistry();

    #endregion




    #region Properties & Fields - Non-Public

    protected override string MemFileName => SMConst.Files.ImageMemFileName;
    protected override string RtxFileName => SMConst.Files.ImageRtxFileName;
    protected override string RtfFileName => null;
    protected override IntPtr RegistryPtr => new IntPtr(SM17Natives.TRegistry.ImageRegistryInstance.Read<int>(Svc.SM.Memory));

    #endregion




    #region Constructors

    protected ImageRegistry() { }

    #endregion




    #region Methods Impl

    protected override Image Create(int        id,
                                    RegMemElem mem,
                                    RegRtElem  rtxOrRtf)
    {
      return new Image(id,
                       mem,
                       rtxOrRtf);
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
