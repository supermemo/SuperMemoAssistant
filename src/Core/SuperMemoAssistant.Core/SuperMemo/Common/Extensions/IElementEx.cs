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




namespace SuperMemoAssistant.SuperMemo.Common.Extensions
{
  using Interop.SuperMemo.Elements.Types;
  using SMA;

  public static class IElementEx
  {
    #region Methods

    /// <summary>Checks whether an element can theoretically be created underneath <paramref name="parent" /></summary>
    /// <param name="parent">The destination branch</param>
    /// <returns></returns>
    public static bool CanAddElement(this IElement parent)
    {
      return parent.ChildrenCount < Core.SM.UI.ElementWdw.LimitChildrenCount;
    }

    /// <summary>
    ///   Calculates how many children can be inserted under <paramref name="parent" /> before reaching the maximum children
    ///   limit.
    /// </summary>
    /// <param name="parent">The destination branch</param>
    /// <returns></returns>
    public static int GetAvailableSlots(this IElement parent)
    {
      return Core.SM.UI.ElementWdw.LimitChildrenCount - parent.ChildrenCount;
    }

    #endregion
  }
}
