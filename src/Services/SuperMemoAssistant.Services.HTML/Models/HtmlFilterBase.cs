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
// Created On:   2019/04/17 13:41
// Modified On:  2019/04/17 13:43
// Modified By:  Alexis

#endregion




using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Forge.Forms;
using Newtonsoft.Json;
using SuperMemoAssistant.Sys.Windows.Input;

namespace SuperMemoAssistant.Services.HTML.Models
{
  public abstract class HtmlFilterBase
  {
    #region Properties & Fields - Public

    public ObservableCollection<HtmlFilter> Children { get; set; } = new ObservableCollection<HtmlFilter>();

    [JsonIgnore]
    public ICommand NewCommand => new AsyncRelayCommand(NewFilter);

    [JsonIgnore]
    public ICommand DeleteCommand => new AsyncRelayCommand<HtmlFilter>(DeleteFilter);

    #endregion




    #region Methods

    private async Task NewFilter()
    {
      var filter = new HtmlFilter();
      var res    = await Show.Window().For<HtmlFilter>(filter);

      if (res.Action is "cancel")
        return;

      Children.Add(filter);
    }

    private async Task DeleteFilter(HtmlFilter filter)
    {
      var res = await Show.Window().For(new Confirmation("Are you sure ?"));

      if (res.Model.Confirmed)
        Children.Remove(filter);
    }

    #endregion
  }
}
