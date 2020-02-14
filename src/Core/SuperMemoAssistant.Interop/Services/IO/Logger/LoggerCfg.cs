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
// Created On:   2020/01/23 08:17
// Modified On:  2020/02/13 21:01
// Modified By:  Alexis

#endregion




using System.ComponentModel;
using Serilog.Events;
using SuperMemoAssistant.Sys.ComponentModel;

namespace SuperMemoAssistant.Services.IO.Logger
{
  public class LoggerCfg : INotifyPropertyChangedEx
  {
    #region Properties & Fields - Public

    public LogEventLevel LogLevel                 { get; set; } = LogEventLevel.Debug;
    public bool          LogFirstChanceExceptions { get; set; }
    public bool          BugReportUserConsent     { get; set; } = false;
    public int           LogMaxSize               { get; set; } = 5242880;

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public bool IsChanged { get; set; }

    #endregion




    #region Events

    /// <inheritdoc />
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
