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
// Created On:   2019/04/13 17:05
// Modified On:  2019/04/13 17:05
// Modified By:  Alexis

#endregion




using System;
using System.Windows.Markup;

namespace SuperMemoAssistant.Sys.Windows
{
  /// <summary>
  /// http://brianlagunas.com/a-better-way-to-data-bind-enums-in-wpf/
  /// </summary>
  public class EnumBindingSourceExtension : MarkupExtension
  {
    #region Properties & Fields - Non-Public

    private Type _enumType;

    #endregion




    #region Constructors

    public EnumBindingSourceExtension() { }

    public EnumBindingSourceExtension(Type enumType)
    {
      EnumType = enumType;
    }

    #endregion




    #region Properties & Fields - Public

    public Type EnumType
    {
      get { return _enumType; }
      set
      {
        if (value != _enumType)
        {
          if (null != value)
          {
            Type enumType = Nullable.GetUnderlyingType(value) ?? value;

            if (!enumType.IsEnum)
              throw new ArgumentException("Type must be for an Enum.");
          }

          _enumType = value;
        }
      }
    }

    #endregion




    #region Methods Impl

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      if (null == _enumType)
        throw new InvalidOperationException("The EnumType must be specified.");

      Type  actualEnumType = Nullable.GetUnderlyingType(_enumType) ?? _enumType;
      Array enumValues     = Enum.GetValues(actualEnumType);

      if (actualEnumType == _enumType)
        return enumValues;

      Array tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
      enumValues.CopyTo(tempArray, 1);
      return tempArray;
    }

    #endregion
  }
}
