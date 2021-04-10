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




namespace SuperMemoAssistant.SuperMemo.Hooks
{
  using System.Collections.Generic;

  public class SMExecRequest<TMetadata>
  {
    #region Constructors

    public SMExecRequest(
      NativeMethod        method,
      bool                shouldHoldMainThread,
      IEnumerable<object> parameters,
      TMetadata           metadata = default)
    {
      Method               = method;
      ShouldHoldMainThread = shouldHoldMainThread;
      Parameters           = parameters;
      Metadata             = metadata;
    }

    #endregion




    #region Properties & Fields - Public

    public NativeMethod        Method               { get; }
    public bool                ShouldHoldMainThread { get; }
    public IEnumerable<object> Parameters           { get; }
    public TMetadata           Metadata             { get; }
    public int                 ExecId               { get; set; }

    #endregion
  }

  public class SMExecRequest : SMExecRequest<object>
  {
    #region Constructors

    public SMExecRequest(NativeMethod method, bool shouldHoldMainThread, IEnumerable<object> parameters, object metadata = default) : base(
      method, shouldHoldMainThread, parameters, metadata) { }

    #endregion
  }
}
