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
// Created On:   2018/07/27 12:55
// Modified On:  2018/11/26 10:29
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using System.Threading.Tasks;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;

namespace SuperMemoAssistant.Interop.SuperMemo.Registry.Members
{
  public interface IRegistryMember
  {
    int    Id       { get; }
    string Name     { get; }
    bool   Empty    { get; }
    int    UseCount { get; }
    

    /// <summary>Retrieve linked file path (HTML, Image, Audio, ...)</summary>
    /// <returns>File path or null</returns>
    string GetFilePath();

    /// <summary>Retrieve elements that are using this registry</summary>
    /// <returns></returns>
    IEnumerable<IElement> GetLinkedElements();

    /// <summary>
    ///   Conveniency method. Will run UI automation to execute action. Delete current member
    ///   from its registry.
    /// </summary>
    /// <returns>Waitable task yielding success result of the operation</returns>
    Task<bool> DeleteAsync();

    /// <summary>
    ///   Conveniency method. Will run UI automation to execute action. Rename current member
    ///   in registry with <paramref name="newName" />.
    /// </summary>
    /// <returns>Waitable task yielding success result of the operation</returns>
    Task<bool> RenameAsync(string newName);

    /// <summary>
    ///   Conveniency method. Will run UI automation to execute action. Starts a neural review
    ///   on given registry member.
    /// </summary>
    /// <returns>Waitable task yielding success result of the operation</returns>
    Task<bool> NeuralAsync();
  }
}
