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
// Modified On:  2020/03/12 15:37
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anotar.Serilog;
using AutoMapper;
using Forge.Forms;
using Forge.Forms.Annotations;

// ReSharper disable StaticMemberInGenericType

namespace SuperMemoAssistant.Services.UI.Configuration
{
  /// <summary>
  ///   Facilitates creating configurations by implementing common behaviour such as
  ///   resetting changes on cancel
  /// </summary>
  /// <typeparam name="TCfg">The child configuration's type</typeparam>
  public abstract class CfgBase<TCfg> : IActionHandler
    where TCfg : class, new()
  {
    #region Constants & Statics

    public const string Cancel = "cancel";
    public const string Save   = "save";

    protected static bool    _isInit;
    protected static IMapper _mapper;

    protected static HashSet<string> _saveActions   = new HashSet<string>();
    protected static HashSet<string> _cancelActions = new HashSet<string>();

    #endregion




    #region Properties & Fields - Non-Public

    protected bool UndoChangesOnCancel { get; }

    /// <summary>
    ///   Override this Property if the configuration model has nested types that need to
    ///   mapped.
    /// </summary>
    protected virtual IEnumerable<Type> InnerTypesToMap { get; } = Array.Empty<Type>();

    #endregion




    #region Constructors

    /// <summary>Constructor</summary>
    /// <param name="undoChangesOnCancel">
    ///   Whether using the <see cref="Cancel" /> Action resets this
    ///   instance to its original version
    /// </param>
    protected CfgBase(bool undoChangesOnCancel = true)
    {
      UndoChangesOnCancel = undoChangesOnCancel;

      if (_isInit == false)
        InitializeCfgBase();
    }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public virtual void HandleAction(IActionContext actionContext)
    {
      var actionName = (string)actionContext.Action;
      var isCancel   = _cancelActions.Contains(actionName);
      var isSave     = _saveActions.Contains(actionName);

      if (isCancel && UndoChangesOnCancel)
        return;

      if (isCancel || isSave)
        ApplyChanges(actionContext.Context as TCfg);
    }

    #endregion




    #region Methods

    /// <summary>Show an option window for this instance of <see cref="TCfg" /></summary>
    /// <param name="options">Optional window display options</param>
    /// <returns>Dialog result</returns>
    public Task<DialogResult<TCfg>> ShowWindow(WindowOptions options = null)
    {
      options ??= new WindowOptions
      {
        CanResize = true,
      };

      return Show.Window(this, options).For<TCfg>(_mapper.Map<TCfg>(this));
    }

    private void InitializeCfgBase()
    {
      // Attributes
      var actionAttributes = typeof(TCfg).GetCustomAttributes(typeof(ActionAttribute), true)
                                         .Cast<ActionAttribute>();

      foreach (var actionAttr in actionAttributes)
        if (actionAttr.IsCancel is true)
          _cancelActions.Add(actionAttr.ActionName);

        else if (actionAttr.IsDefaultAttribute() || actionAttr.ClosesDialog is true)
          _saveActions.Add(actionAttr.ActionName);

      // Mapper
      var mapperCfg = new MapperConfiguration(cfg =>
      {
        cfg.CreateMap<TCfg, TCfg>();

        foreach (var type in InnerTypesToMap)
          cfg.CreateMap(type, type);
      });

      _mapper = mapperCfg.CreateMapper();

      // Set init flag
      _isInit = true;
    }

    private void ApplyChanges(TCfg original)
    {
      if (original == null)
      {
        LogTo.Error($"original cannot be NULL for type {typeof(TCfg).FullName}");
        throw new ArgumentNullException(nameof(original));
      }

      _mapper.Map(this, original);
    }

    #endregion
  }
}
