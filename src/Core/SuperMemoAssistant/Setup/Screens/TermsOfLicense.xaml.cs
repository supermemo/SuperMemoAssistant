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
// Modified On:  2020/04/10 14:15
// Modified By:  Alexis

#endregion






// ReSharper disable RedundantNameQualifier

namespace SuperMemoAssistant.Setup.Screens
{
  using SMA.Configs;
  using Sys.ComponentModel;

  /// <summary>Compulsory license agreement screen.</summary>
  public partial class TermsOfLicense : SMASetupScreenBase, INotifyPropertyChangedEx
  {
    #region Constructors

    public TermsOfLicense(CoreCfg startupCfg)
    {
      StartupCfg = startupCfg;

      InitializeComponent();
    }

    #endregion




    #region Properties & Fields - Public

    public CoreCfg StartupCfg { get; }

    /// <summary>Proxy for the agreement state, used to raise property changed on <see cref="IsSetup" /></summary>
    public bool HasAgreedToTermsOfLicense
    {
      get => StartupCfg.HasAgreedToTermsOfLicense;
      set
      {
        StartupCfg.HasAgreedToTermsOfLicense = value;
        OnPropertyChanged(nameof(IsSetup));
      }
    }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public override bool IsSetup => HasAgreedToTermsOfLicense;

    /// <inheritdoc />
    public override string ListTitle => "License";

    /// <inheritdoc />
    public override string WindowTitle => "License agreement";

    /// <inheritdoc />
    public override string Description { get; } = null;

    /// <inheritdoc />
    public bool IsChanged { get; set; }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public override void OnDisplayed() { }

    /// <inheritdoc />
    public override void OnNext()
    {
      if (IsChanged)
      {
        SuperMemoAssistant.SMA.Core.Configuration.Save<CoreCfg>(StartupCfg);

        IsChanged = false;
      }
    }

    #endregion
  }
}
