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
// Created On:   2018/05/30 23:19
// Modified On:  2018/05/30 23:19
// Modified By:  Alexis

#endregion




using System;
using System.Linq;
using System.Reflection;

namespace SuperMemoAssistant.Sys
{
  public class InitOnLoad : Attribute
  {
    #region Methods

    /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded. </exception>
    /// <exception cref="TargetException">In the .NET for Windows Store apps or the Portable Class Library, catch <see cref="T:System.Exception" /> instead. The field is non-static and obj is <see langword="null" />. </exception>
    /// <exception cref="FieldAccessException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.The caller does not have permission to access this field. </exception>
    /// <exception cref="AppDomainUnloadedException">The operation is attempted on an unloaded application domain. </exception>
    public static void Initialize()
    {
      // get a list of types which are marked with the InitOnLoad attribute
      var types =
        from t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
        where t.GetCustomAttributes(typeof(InitOnLoad), false).Any()
        select t;

      // process each type to force initialise it
      foreach (var type in types)
      {
        // try to find a static field which is of the same type as the declaring class
        var field = type.GetFields(BindingFlags.Static | BindingFlags.Public
                               | BindingFlags.NonPublic)
                    .FirstOrDefault(f => f.FieldType == type);
        // evaluate the static field if found
        if (field != null) field.GetValue(null);
      }
    }

    #endregion
  }
}
