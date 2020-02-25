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
// Modified On:  2020/02/22 15:54
// Modified By:  Alexis

#endregion




using System;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.Learning;

namespace SuperMemoAssistant.Interop.SuperMemo.UI.Element
{
  public interface IElementWdw : IWdw
  {
    bool ActivateWindow();

    IControlGroup ControlGroup { get; }

    int      CurrentElementId { get; }
    IElement CurrentElement   { get; }

    short        LimitChildrenCount    { get; }
    int          CurrentConceptGroupId { get; }
    int          CurrentRootId         { get; set; }
    int          CurrentHookId         { get; set; }
    int          CurrentConceptId      { get; }
    LearningMode CurrentLearningMode   { get; }

    bool SetCurrentConcept(int conceptId);
    bool GoToElement(int       elementId);

    bool PasteArticle();
    bool PasteElement();
    int  AppendElement(ElementType elementType);
    bool AddElementFromText(string elementDesc);

    int GenerateExtract(ElementType elementType,
                        bool        memorize                  = true,
                        bool        askUserToScheduleInterval = false);

    int GenerateCloze(bool memorize                  = true,
                      bool askUserToScheduleInterval = false);

    bool Delete();
    bool Done();

    event Action<SMDisplayedElementChangedArgs> OnElementChanged;

    bool NextElementInLearningQueue();
    bool SetElementState(int          state);
    bool PostponeRepetition(int       interval);
    bool ForceRepetition(int          interval, bool adjustPriority);
    bool ForceRepetitionAndResume(int interval, bool adjustPriority);
  }
}
