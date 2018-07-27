using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Interop.mshtml;
using Newtonsoft.Json;
using SuperMemoAssistant.Interop.Sys;
using SuperMemoAssistant.SuperMemo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
//using SuperMemoAssistant.COM.InternetExplorer;
//using SuperMemoAssistant.SuperMemo.Common.Core;
//using SuperMemoAssistant.SuperMemo.Common.Files;
//using SuperMemoAssistant.SuperMemo17.Core;
using System.Linq;
using System.Windows.Input;

namespace SuperMemoAssistant.Console
{
  class Program
  {
    static void Main(string[] args)
    {
      var smCol = new SMCollection("SMATest", SMConst.CollectionPath);
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
        var elWdw = windows.FirstOrDefault(w => w.ClassName == "TElWind");

        //var learnBar = contentWdw.FindChildAt(2).FindFirstChild();
        //var pasteArticleBtn = learnBar.FindChildAt(4);
        //var learnBtn = contentWdw.FindFirstChild().AsButton();
        //var menus = windows[1].FindAllChildren(cf => cf.Menu());
        //Keyboard.Focus()

        var ieServer = elWdw.FindFirstDescendant(c => c.ByClassName("Internet Explorer_Server"));
        var ieHwnd = ieServer.FrameworkAutomationElement.NativeWindowHandle.Value;
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
