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
// Created On:   2020/03/29 00:20
// Modified On:  2020/04/09 15:38
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.SuperMemo.Common.Content.Components
{
  using Interop.SuperMemo.Content.Components;
  using Interop.SuperMemo.Content.Models;
  using Interop.SuperMemo.Registry.Members;
  using SMA;
  using SuperMemo17.Files;

  internal class ComponentVideo : ComponentBase, IComponentVideo
  {
    #region Properties & Fields - Non-Public

    protected int VideoId { get; set; }

    #endregion




    #region Constructors

    public ComponentVideo(ref InfComponentsVideo17 comp)
      : base(comp.left, comp.top, comp.width, comp.height, (AtFlags)comp.displayAt)
    {
      VideoId      = SetValue(comp.registryId, nameof(VideoId));
      IsContinuous = SetValue(comp.isContinuous, nameof(IsContinuous));
      IsFullScreen = SetValue(comp.isFullScreen, nameof(IsFullScreen));
      ExtractStart = SetValue(comp.extractStart, nameof(ExtractStart));
      ExtractStop  = SetValue(comp.extractStop, nameof(ExtractStop));
      Panel        = SetValue((MediaPanelType)comp.panel, nameof(Panel));
    }

    #endregion




    #region Properties Impl - Public

    public IVideo         Video        => Core.SM.Registry.Video?[VideoId];
    public bool           IsContinuous { get; set; }
    public bool           IsFullScreen { get; set; }
    public uint           ExtractStart { get; set; }
    public uint           ExtractStop  { get; set; }
    public MediaPanelType Panel        { get; set; }

    #endregion




    #region Methods

    public void Update(ref InfComponentsVideo17 comp)
    {
      ComponentFieldFlags flags = ComponentFieldFlags.None;

      VideoId      = SetValue(VideoId, comp.registryId, nameof(VideoId), ref flags);
      IsContinuous = SetValue(IsContinuous, comp.isContinuous, nameof(IsContinuous), ref flags);
      IsFullScreen = SetValue(IsFullScreen, comp.isFullScreen, nameof(IsFullScreen), ref flags);
      ExtractStart = SetValue(ExtractStart, comp.extractStart, nameof(ExtractStart), ref flags);
      ExtractStop  = SetValue(ExtractStop, comp.extractStop, nameof(ExtractStop), ref flags);
      Panel        = SetValue(Panel, (MediaPanelType)comp.panel, nameof(Panel), ref flags);

      base.Update(
        comp.left, comp.top,
        comp.width, comp.height,
        (AtFlags)comp.displayAt,
        flags
      );
    }

    #endregion
  }
}
