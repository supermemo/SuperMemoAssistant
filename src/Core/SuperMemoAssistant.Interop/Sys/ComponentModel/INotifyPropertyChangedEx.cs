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
// Modified On:  2020/03/11 15:25
// Modified By:  Alexis

#endregion




using System.ComponentModel;
using Newtonsoft.Json;

namespace SuperMemoAssistant.Sys.ComponentModel
{
  /// <summary>
  ///   Extends <see cref="INotifyPropertyChanged" /> interface to add
  ///   <see cref="IsChanged" /> for usage with Fody.PropertyChanged.
  ///   https://github.com/Fody/PropertyChanged/wiki/Implementing-An-IsChanged-Flag
  /// </summary>
  public interface INotifyPropertyChangedEx : INotifyPropertyChanged
  {
    /// <summary>
    ///   This flag is set to true by Fody.PropertyChanged when any weaved property's value is
    ///   changed.
    /// </summary>
    [JsonIgnore]
    bool IsChanged { get; set; }
  }
}
