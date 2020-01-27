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
// Modified On:  2020/01/26 18:24
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace SuperMemoAssistant.Extensions
{
  public static class DependencyObjectEx
  {
    #region Methods

    public static IEnumerable<DependencyObject> EnumerateChildren(this DependencyObject parent)
    {
      for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
      {
        // retrieve child at specified index
        var directChild = (Visual)VisualTreeHelper.GetChild(parent, i);

        // return found child
        yield return directChild;

        // return all children of the found child
        foreach (var nestedChild in directChild.EnumerateChildren())
          yield return nestedChild;
      }
    }

    public static IEnumerable<T> EnumerateChildren<T>(this DependencyObject parent)
      where T : class
    {
      foreach (DependencyObject child in parent.EnumerateChildren())
      {
        if (child is T typedChild)
          yield return typedChild;

        foreach (T descendant in EnumerateChildren<T>(child))
          yield return descendant;
      }
    }

    public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
    {
      //get parent item
      DependencyObject parentObject = VisualTreeHelper.GetParent(child);

      //we've reached the end of the tree
      if (parentObject == null) return null;

      //check if the parent matches the type we're looking for
      if (parentObject is T parent)
        return parent;

      return FindParent<T>(parentObject);
    }

    #endregion
  }
}
