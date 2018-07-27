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
// Created On:   2018/05/02 16:03
// Modified On:  2018/07/27 15:50
// Modified By:  Alexis

#endregion




using System.Linq;
using FlaUI.Core;
using FlaUI.UIA3;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.SuperMemo;
//using SuperMemoAssistant.COM.InternetExplorer;
//using SuperMemoAssistant.SuperMemo.Common.Core;
//using SuperMemoAssistant.SuperMemo.Common.Files;
//using SuperMemoAssistant.SuperMemo17.Core;

namespace SuperMemoAssistant.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var smCol = new SMCollection("SMATest",
                                         SMConst.CollectionPath);
            bool ret = SMA.Instance.Start(smCol);

            global::System.Console.ReadKey();
        }

#if false
    static void Main(string[] args)
    {
      Random rnd = new Random();
      int i = 3;

      do
      {
        System.Console.WriteLine("Here 1");

        if (rnd.Next(i + 1) != i)
        {
          System.Console.WriteLine("Here 2");
          continue;
        }

        System.Console.WriteLine("Here 3");
        break;
      } while (true);

      System.Console.WriteLine("There");
      System.Console.ReadKey();
    }
#endif

#if false
    const int count = 100000000;

    public static object Memory { get; private set; }

    static void Main(string[] args)
    {
      System.Console.WriteLine(string.Format("count: {0}", count));

      MeasureFunction(FillUnmanagedArray, "unsafe array");
      MeasureFunction(FillArray, "array");
      MeasureFunction(FillListWithoutAllocation, "without allocation");
      MeasureFunction(FillListWithAllocation, "with allocation");

      string input = System.Console.ReadLine();
    }

    static void MeasureFunction(Action function, string name)
    {
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
      function();
      stopwatch.Stop();
      System.Console.WriteLine(string.Format("Function {0} finished after \t {1}ms", name, stopwatch.ElapsedMilliseconds, count));
    }

    static void FillListWithoutAllocation()
    {
      List<int> list = new List<int>();
      for (int i = 0; i < count; i++) list.Add(i);
    }

    static void FillListWithAllocation()
    {
      List<int> list = new List<int>(count);
      for (int i = 0; i < count; i++) list.Add(i);
    }

    static void FillArray()
    {
      int[] array = new int[count];
      for (int i = 0; i < count; i++) array[i] = i;
    }

    static void FillUnmanagedArray()
    {
      unsafe
      {
        int[] array = new int[count];
        fixed (int* ptr = array)

          for (int i = 0; i < count; i++) *ptr = i;
      }
    }
#endif

#if false
    static void Main(string[] args)
    {
      //
      // ReverseEngineer collection
      string conceptMemFilePath = "D:\\Work\\IT\\Prog\\SuperMemo\\SuperMemoRE\\concepts\\latest\\Concept.mem";
      string conceptRtxFilePath = "D:\\Work\\IT\\Prog\\SuperMemo\\SuperMemoRE\\concepts\\latest\\Concept.rtx";

      string textMemFilePath = "D:\\Work\\IT\\Prog\\SuperMemo\\SuperMemoRE\\texts\\latest\\Text.mem";
      string textRtxFilePath = "D:\\Work\\IT\\Prog\\SuperMemo\\SuperMemoRE\\texts\\latest\\Text.rtx";

      //
      // Test collection
      //string conceptMemFilePath = "D:\\SuperMemo\\Test\\Test\\registry\\Concept.mem";
      //string conceptRtxFilePath = "D:\\SuperMemo\\Test\\Test\\registry\\Concept.rtx";

      //string textMemFilePath = "D:\\SuperMemo\\Test\\Test\\registry\\Text.mem";
      //string textRtxFilePath = "D:\\SuperMemo\\Test\\Test\\registry\\Text.rtx";

      //
      // Main collection
      //string conceptMemFilePath = "D:\\SuperMemo\\Alexis\\Main\\registry\\Concept.mem";
      //string conceptRtxFilePath = "D:\\SuperMemo\\Alexis\\Main\\registry\\Concept.rtx";

      //string textMemFilePath = "D:\\SuperMemo\\Alexis\\Main\\registry\\Image.mem";
      //string textRtxFilePath = "D:\\SuperMemo\\Alexis\\Main\\registry\\Image.rtx";

      /*
      System.Console.WriteLine("Reading concept registry...");
      var concepts = RegistryLoader.LoadRegistry<Concept>(conceptMemFilePath, conceptRtxFilePath);
      System.Console.WriteLine("Dumping concepts...");
      System.Console.WriteLine(JsonConvert.SerializeObject(concepts, Formatting.Indented));

      System.Console.WriteLine("Reading text registry...");
      var texts = RegistryLoader.LoadRegistry<Concept>(textMemFilePath, textRtxFilePath);
      System.Console.WriteLine("Dumping texts...");
      System.Console.WriteLine(JsonConvert.SerializeObject(texts, Formatting.Indented));

      System.Console.WriteLine(concepts.Count);
      */
      UIAutomationTest();

      System.Console.ReadKey();
    }
#endif
        private static void UIAutomationTest()
        {
            using (var ui = new UIA3Automation())
            {
                var app = Application.Attach("sm17.exe");


                //var main = app.GetMainWindow(ui);
                var windows = app.GetAllTopLevelWindows(ui);
                var elWdw   = windows.FirstOrDefault(w => w.ClassName == "TElWind");

                //var learnBar = contentWdw.FindChildAt(2).FindFirstChild();
                //var pasteArticleBtn = learnBar.FindChildAt(4);
                //var learnBtn = contentWdw.FindFirstChild().AsButton();
                //var menus = windows[1].FindAllChildren(cf => cf.Menu());
                //Keyboard.Focus()

                var ieServer = elWdw.FindFirstDescendant(c => c.ByClassName("Internet Explorer_Server"));
                var ieHwnd   = ieServer.FrameworkAutomationElement.NativeWindowHandle.Value;
                //IHTMLDocument2 ieDocument = IEComHelper.GetDocumentFromHwnd(ieHwnd);
                //IHTMLWindow2 ieWindow = ieDocument.parentWindow;

                //((IHTMLDocument5)ieDocument).
                //ieWindow.execScript("alert('This is a test');");

                //ieDocument.url = "http://google.com";

                windows.ToString();
            }
        }
    }
}
