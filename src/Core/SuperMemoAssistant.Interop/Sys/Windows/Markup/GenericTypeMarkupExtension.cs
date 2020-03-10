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
// Modified On:  2020/02/28 13:23
// Modified By:  Alexis

#endregion




using System;
using System.Windows.Markup;

namespace SuperMemoAssistant.Sys.Windows.Markup
{
  /*
   * Original from https://stackoverflow.com/questions/54092789/datatemplates-and-generics/54124755#54124755
   */

  /// <summary>
  /// Enable xaml markup language to enable the usage of Generic types
  /// 
  /// Usage:
  /// &lt;x:Array Type="{x:Type System:Type}" x:Key="StringObjectParameters"&gt;
  ///   &lt;x:Type TypeName="System:String" /&gt;
  ///   &lt;x:Type TypeName="System:Object" /&gt;
  /// &lt;/x:Array&gt;
  ///
  /// &lt;markup:XamlGenericType BaseType="{x:Type TypeName=generic:Dictionary`2}"
  /// InnerTypes="{StaticResource StringObjectParameters}"
  /// x:Key="DictionaryStringObjectType" /&gt;
  /// 
  /// </summary>
  public class XamlGenericType : MarkupExtension
  {
    #region Constructors

    public XamlGenericType() { }

    public XamlGenericType(Type baseType, params Type[] innerTypes)
    {
      BaseType   = baseType;
      InnerTypes = innerTypes;
    }

    #endregion




    #region Properties & Fields - Public

    /// <summary>
    /// The base type to which parametrized types are added.
    /// E.g. in List&gt;string&lt;, List is the Base Type.
    /// </summary>
    public Type BaseType { get; set; }

    /// <summary>
    /// The generic parameter. Use for generic types that only accept a single parameter.
    /// E.g. in List&gt;string&lt;, string is the Inner Type.
    /// </summary>
    public Type InnerType { get; set; }

    /// <summary>
    /// The generic parameters. Use for generic types that accept two or more parameters.
    /// E.g. in Dictionary&gt;string, object&lt;, string and object are the Inner Types.
    /// Usage:
    /// &lt;x:Array Type="{x:Type System:Type}" x:Key="StringObjectParameters"&gt;
    ///   &lt;x:Type TypeName="System:String" /&gt;
    ///   &lt;x:Type TypeName="System:Object" /&gt;
    /// &lt;/x:Array&gt;
    ///
    /// &lt;markup:XamlGenericType BaseType="{x:Type TypeName=generic:Dictionary`2}"
    /// InnerTypes="{StaticResource StringObjectParameters}"
    /// x:Key="DictionaryStringObjectType" /&gt;
    /// </summary>
    public Type[] InnerTypes { get; set; }

    #endregion


    

    #region Methods Impl

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      if (InnerType != null && InnerTypes != null)
        throw new InvalidOperationException("InnerType and InnerTypes cannot both be defined at the same time.");

      Type result;

      if (InnerType != null)
        result = BaseType.MakeGenericType(InnerType);

      else if (InnerTypes != null || InnerTypes.Length > 0)
        result = BaseType.MakeGenericType(InnerTypes);

      else
        throw new InvalidOperationException("One of InnerType or InnerTypes must be defined. InnerTypes must not be an empty array.");

      return result;
    }

    #endregion
  }
}
