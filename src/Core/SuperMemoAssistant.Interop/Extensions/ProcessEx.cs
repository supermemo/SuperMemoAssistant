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
// Created On:   2018/06/03 00:00
// Modified On:  2018/06/06 02:06
// Modified By:  Alexis

#endregion




using System.Diagnostics;
using System.Text;

namespace SuperMemoAssistant.Extensions
{
  public static class ProcessEx
  {
    #region Methods

    public static System.Diagnostics.Process CreateBackgroundProcess(string binName,
                                                                     string args,
                                                                     string workingDirectory = null)
    {
      System.Diagnostics.Process p = new System.Diagnostics.Process
      {
        StartInfo =
        {
          FileName        = binName,
          Arguments       = args,
          UseShellExecute = false,
          CreateNoWindow  = true,
        }
      };

      if (workingDirectory != null)
        p.StartInfo.WorkingDirectory = workingDirectory;

      return p;
    }

    public static (int exitCode, string output, bool timedOut) ExecuteBlockingWithOutputs(this System.Diagnostics.Process p,
                                                                                          int timeout =
                                                                                            int.MaxValue,
                                                                                          bool kill = true)
    {
      StringBuilder outputBuilder = new StringBuilder();

      void OutputDataReceived(object                sender,
                              DataReceivedEventArgs e)
      {
        lock (outputBuilder)
          outputBuilder.Append(e.Data + "\n");
      }

      p.EnableRaisingEvents              =  true;
      p.StartInfo.RedirectStandardOutput =  true;
      p.StartInfo.RedirectStandardError  =  true;
      p.OutputDataReceived               += OutputDataReceived;
      p.ErrorDataReceived                += OutputDataReceived;

      try
      {
        p.Start();

        p.BeginOutputReadLine();
        p.BeginErrorReadLine();

        if (!p.WaitForExit(timeout))
        {
          if (kill)
            p.Kill();

          return (-1, null, true);
        }

        p.WaitForExit();

        return (p.ExitCode, outputBuilder.ToString(), false);
      }
      finally
      {
        p.Dispose();
      }
    }

    #endregion
  }
}
