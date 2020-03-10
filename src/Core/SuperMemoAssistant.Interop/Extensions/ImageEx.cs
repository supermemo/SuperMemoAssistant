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
// Created On:   2019/01/14 19:15
// Modified On:  2019/01/24 11:54
// Modified By:  Alexis

#endregion




using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SuperMemoAssistant.Extensions
{
  public static class ImageEx
  {
    #region Methods

    public static string ToBase64(this Image  image,
                                  ImageFormat format)
    {
      using (MemoryStream ms = new MemoryStream())
      {
        image.Save(ms, format);
        return Convert.ToBase64String(ms.ToArray());
      }
    }

    public static Image FromBase64(string base64)
    {
      using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64)))
      {
        ms.Seek(0, SeekOrigin.Begin);

        return Image.FromStream(ms);
      }
    }

    #endregion
  }
}
