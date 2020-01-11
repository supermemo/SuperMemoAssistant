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
// Created On:   2019/08/09 18:44
// Modified On:  2019/12/14 20:00
// Modified By:  Alexis

#endregion




using Process.NET.Memory;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace SuperMemoAssistant.SuperMemo.SuperMemo17
{
  public partial class SM17Natives
  {
    public partial class TElWind17
    {
      public class TComponentData17 : TComponentData
      {
        /// <inheritdoc />
        public TComponentData17(TElWind17 elWind17)
        {
          var componentsDataPtr = elWind17.ComponentsDataPtr;

          ComponentDataArrOffset     = 0x003F;
          ComponentDataArrItemLength = 0x0D;
          ComponentCountPtr          = new ObjPtr(componentsDataPtr, 0x000A);
          IsModifiedPtr              = new ObjPtr(componentsDataPtr, 0x0013);
        }




        #region Constants & Statics

        public override int ComponentDataArrOffset     { get; }
        public override int ComponentDataArrItemLength { get; }

        public override ObjPtr ComponentCountPtr { get; }
        public override ObjPtr IsModifiedPtr     { get; }


        //
        // Call Signatures
#if false
// TComponentData.GetType
        public IMemoryPattern GetTypeCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 3A 45 EF",
          1);
        // TComponentData.GetText
        public IMemoryPattern GetTextCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 83 7D F4 00 74 25 8B 0D ? ? ? ?",
          1);
        // TComponentData.SetText
        public IMemoryPattern SetTextCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 0F BE 55 F3",
          1);
        // TComponentData.GetTextItem
        public IMemoryPattern GetTextRegMemberCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 89 45 E8 8A 55 DE",
          1);
        // TComponentData.SetTextItem
        public IMemoryPattern SetTextRegMemberCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 8A 55 EF 8B 45 F0",
          1);
        // TComponentData.GetImageItem
        public IMemoryPattern GetImageRegMemberCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 89 45 F4 8B 45 F4 8B E5 5D C3 90 55 8B EC 83 C4 F4 89 4D F4 88 55 FB 89 45 FC 8B 4D F4 8A 55 FB 8B 45 FC 8B 80 ? ? ? ? E8 ? ? ? ? 8B E5 5D C3 90 55 8B EC 51",
          1);
        // TComponentData.SetImageItem
        public IMemoryPattern SetImageRegMemberCallSig = new DwordCallPattern(
          "E8 ? ? ? ? 88 45 F3 EB 4A",
          1);
#endif

        #endregion




        #region Properties & Fields - Public

#if false
        public Procedure<Func<int, IntPtr, int>> GetImageRegMember { get; } = new Procedure<Func<int, IntPtr, int>>(
          "GetImageRegMember",
          CallingConventions.Register,
          GetImageRegMemberCallSig
        );

        public Procedure<Func<IntPtr, int, string>> GetText { get; } = new Procedure<Func<IntPtr, int, string>>(
          "GetText",
          CallingConventions.Register,
          GetTextCallSig
        );

        public Procedure<Func<int, IntPtr, int>> GetTextRegMember { get; } = new Procedure<Func<int, IntPtr, int>>(
          "GetTextRegMember",
          CallingConventions.Register,
          GetTextRegMemberCallSig
        );
        public Procedure<Action<IntPtr, int, int>> SetImageRegMember { get; } = new Procedure<Action<IntPtr, int, int>>(
          "SetImageRegMember",
          CallingConventions.Register,
          SetImageRegMemberCallSig
        );
        public Procedure<Action<IntPtr, int, DelphiUString>> SetText { get; } = new Procedure<Action<IntPtr, int, DelphiUString>>(
          "SetText",
          CallingConventions.Register,
          SetTextCallSig
        );
        public Procedure<Action<int, int, IntPtr>> SetTextRegMember { get; } = new Procedure<Action<int, int, IntPtr>>(
          "SetText",
          CallingConventions.Register,
          SetTextRegMemberCallSig
        );
#endif

        #endregion
      }
    }
  }
}
