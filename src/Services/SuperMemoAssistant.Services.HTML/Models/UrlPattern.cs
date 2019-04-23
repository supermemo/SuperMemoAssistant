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
// Created On:   2019/04/22 15:15
// Modified On:  2019/04/22 17:34
// Modified By:  Alexis

#endregion




using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Forge.Forms.Annotations;
using Forge.Forms.Validation;
using Newtonsoft.Json;
using SuperMemoAssistant.Services.HTML.Extensions;

namespace SuperMemoAssistant.Services.HTML.Models
{
  [Serializable]
  [Form(Mode = DefaultFields.None)]
  [Title("Url pattern Settings",
    IsVisible = "{Env DialogHostContext}")]
  [DialogAction("cancel",
    "Cancel",
    IsCancel = true)]
  [DialogAction("save",
    "Save",
    IsDefault = true,
    Validates = true)]
  public class UrlPattern : INotifyPropertyChanged
  {
    #region Properties & Fields - Non-Public

    private string _patternError = null;

    #endregion




    #region Properties & Fields - Public

    [Field(Name = "Type")]
    [SelectFrom(typeof(UrlPatternType),
      SelectionType = SelectionType.ComboBox)]
    public UrlPatternType Type { get; set; }

    [Field(Name = "Pattern")]
    [Value(Must.NotBeEmpty)]
    [Value(Must.SatisfyMethod, nameof(Validate), Message = "{Binding FilterError}")]
    public string Pattern { get; set; }

    [Field(Name = "Priority")]
    public int Priority { get; set; }


    //
    // Helpers

    [JsonIgnore]
    public string PatternError => _patternError ?? "Unknown error";

    [JsonIgnore]
    public Regex Regex { get; private set; }

    #endregion




    #region Methods Impl

    public override string ToString()
    {
      return $"({Priority}) {Type}: {Pattern}";
    }

    #endregion




    #region Methods

    public static bool Validate(ValidationContext validationContext)
    {
      var urlPattern = (UrlPattern)validationContext.Model;

      switch (validationContext.PropertyName)
      {
        case nameof(Pattern):
          var ret = urlPattern.ValidateFilter(out urlPattern._patternError);
          urlPattern.PropertyChanged?.Invoke(urlPattern, new PropertyChangedEventArgs(nameof(PatternError)));

          return ret;
      }

      return false;
    }

    public void OnPatternChanged(object _, object after)
    {
      if (Type == UrlPatternType.Regex)
        try
        {
          Regex = new Regex((string)after, RegexOptions.Compiled);
        }
        catch
        {
          Regex = null;
        }

      else
        Regex = null;
    }

    #endregion




    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
