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




// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

namespace SuperMemoAssistant.SuperMemo.Natives
{
  public partial class SMNatives
  {
    #region Constructors

    public SMNatives(NativeData nativeData)
    {
      Globals     = new TGlobals(nativeData);
      Application = new TApplication(nativeData);
      Control     = new TControl(nativeData);
      Contents    = new TContents(nativeData);
      ElWind      = new TElWind(nativeData);
      SMMain      = new TSMMain(nativeData);
      Database    = new TDatabase(nativeData);
      Registry    = new TRegistry(Database, nativeData);
      FileSpace   = new TFileSpace(Database, nativeData);
      Queue       = new TQueue(nativeData);
    }

    #endregion




    #region Properties & Fields - Public

    public TGlobals     Globals     { get; }
    public TApplication Application { get; }
    public TControl     Control     { get; }
    public TElWind      ElWind      { get; }
    public TSMMain      SMMain      { get; }
    public TDatabase    Database    { get; }
    public TContents    Contents    { get; }
    public TRegistry    Registry    { get; }
    public TFileSpace   FileSpace   { get; }
    public TQueue       Queue       { get; }

    #endregion
  }
}
