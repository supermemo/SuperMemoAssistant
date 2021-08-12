using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;
using SuperMemoAssistant.SMA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.SuperMemo.Common.Content.Controls
{
  public class ControlSound : ComponentControlBase, IControlSound

  {
    /// <inheritdoc />
    public ControlSound(int          id,
                        ControlGroup group)
      : base(id,
             ComponentType.Sound,
             group) { }



    #region Properties Impl - Public

    /// <inheritdoc />
    public ISound SoundMember
    {
      get => Core.SM.Registry.Sound[SoundMemberId];
      set => SoundMemberId = value?.Id ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc />
    public int SoundMemberId
    {
      get => Group.GetSoundRegMember(this);
      set => Group.SetSoundRegMember(this, value);
    }

    #endregion
  }
}
