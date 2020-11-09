using Anotar.Serilog;
using SuperMemoAssistant.SMA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.SuperMemo.Natives
{
  public partial class SMNatives
  {
    public class TContents
    {
      #region Constructor
      public TContents(NativeData nativeData)
      {
        InstancePtr = new IntPtr(nativeData.Pointers[NativePointer.TContents_InstancePtr]);
      }
      #endregion


      #region Methods
      public bool MoveElementToConcept(IntPtr TContentPtr, int elementId, int conceptId)
      {
        try
        {
          NativeMethod.TContents_MoveElementToConcept
                        .ExecuteOnMainThread(TContentPtr,
                                             elementId,
                                             conceptId);

          return true;
        }
        catch (Exception ex)
        {
          LogTo.Error(ex, "Native method call threw an exception.");
          return false;
        }
      }
      #endregion

      #region Properties & Fields - Public
      public IntPtr InstancePtr { get; }
      #endregion

    }

  }
}