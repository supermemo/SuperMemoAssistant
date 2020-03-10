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
// Modified On:  2020/02/17 17:46
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperMemoAssistant.Extensions;

namespace SuperMemoAssistant.Sys.Remoting
{
  public partial class RemoteTask
  {
    #region Methods

    public static Task WhenAll(IEnumerable<RemoteTask> remoteTasks)
    {
      var tasks = remoteTasks.Select(rt => rt.GetTask());

      return Task.WhenAll(tasks);
    }

    public static Task<T[]> WhenAll<T>(IEnumerable<RemoteTask<T>> remoteTasks)
    {
      var tasks = remoteTasks.Select(rt => rt.GetTask<T>());

      return Task.WhenAll(tasks);
    }

    #endregion
  }
}
