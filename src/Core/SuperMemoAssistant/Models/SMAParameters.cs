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
// Created On:   2020/02/12 22:32
// Modified On:  2020/02/12 23:23
// Modified By:  Alexis

#endregion




using System;
using CommandLine;

// ReSharper disable StringLiteralTypo

namespace SuperMemoAssistant.Models
{
  public class SMAParameters
  {
    #region Properties & Fields - Public

    [Option("developer", Required = false,
            HelpText              = "For SMA developers only. Disables certain mechanisms of SMA which make developing it impossible")]
    public bool Developer { get; set; }

    [Option('c', "collection", Required = false,
            HelpText                    = "Will bypass collection selection if a valid path to a collection .kno file is passed")]
    public string CollectionKnoPath { get; set; }

    [Option("key-logger", Required = false,
            HelpText               = "Debug feature. Logs all keys send with a modifier (ctrl, alt, shift, meta) to file.")]
    public bool KeyLogger { get; set; }

    //
    // Installer parameters

    [Option("squirrel-install", Required = false, Hidden = true)]
    public Version SquirrelInstalled { get; set; }

    [Option("squirrel-updated", Required = false, Hidden = true)]
    public Version SquirrelUpdated { get; set; }

    [Option("squirrel-obsolete", Required = false, Hidden = true)]
    public Version SquirrelObsoleted { get; set; }

    [Option("squirrel-uninstall", Required = false, Hidden = true)]
    public Version SquirrelUninstalled { get; set; }

    [Option("squirrel-firstrun", Required = false, Hidden = true)]
    public bool SquirrelFirstRun { get; set; }

    #endregion
  }
}
