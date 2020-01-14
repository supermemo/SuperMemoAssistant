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
// Created On:   2018/11/23 19:54
// Modified On:  2018/11/23 20:08
// Modified By:  Alexis

#endregion




using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace SuperMemoAssistant.Sys.Drawing
{
  [Serializable]
  public class ImageWrapper
  {
    #region Properties & Fields - Non-Public

    private readonly byte[] _bytes;

    #endregion




    #region Constructors

    public ImageWrapper(Image img)
    {
      if (img == null)
        throw new ArgumentNullException(nameof(img));

      using MemoryStream stream = new MemoryStream();

      img.Save(stream,
               ImageFormat.Png);
      _bytes = stream.ToArray();
    }

    #endregion




    #region Methods

    public FileInfo ToFile(string filePath)
    {
      return ToFile(filePath,
                    ImageFormat.Png);
    }

    public FileInfo ToFile(string filePath, ImageFormat fmt)
    {
      ToBitmap().Save(filePath, fmt);

      return new FileInfo(filePath);
    }

    public BitmapImage ToBitmapImage()
    {
      BitmapImage img = new BitmapImage();

      img.BeginInit();
      img.StreamSource = new MemoryStream(_bytes);
      img.EndInit();

      return img;
    }

    public Bitmap ToBitmap()
    {
      return new Bitmap(new MemoryStream(_bytes));
    }

    #endregion
  }
}
