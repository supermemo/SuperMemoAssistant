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
// Modified On:  2020/02/02 22:51
// Modified By:  Alexis

#endregion




using System;

namespace SuperMemoAssistant.Sys.Windows.Search
{
  [Flags]
  [Serializable]
  public enum WindowsSearchKind
  {
    // KIND_CALENDAR
    Calendar = 1,
    // KIND_COMMUNICATION
    Communication = 2,
    // KIND_CONTACT
    Contact = 4,
    // KIND_DOCUMENT
    Document = 8,
    // KIND_EMAIL
    Email = 16,
    // KIND_FEED
    Feed = 32,
    // KIND_FOLDER
    Folder = 64,
    // KIND_GAME
    Game = 128,
    // KIND_INSTANTMESSAGE
    InstantMessage = 256,
    // KIND_JOURNAL
    Journal = 512,
    // KIND_LINK
    Link = 1024,
    // KIND_MOVIE
    Movie = 2048,
    // KIND_MUSIC
    Music = 4096,
    // KIND_NOTE
    Note = 8192,
    // KIND_PICTURE
    Picture = 16384,
    // KIND_PROGRAM
    Program = 32768,
    // KIND_RECORDEDTV
    RecordedTv = 131072,
    // KIND_SEARCHFOLDER
    SearchFolder = 262144,
    // KIND_TASK
    Task = 524288,
    // KIND_VIDEO
    Video = 1048576,
    // KIND_WEBHISTORY
    WebHistory = 2097152,

    // other
    File = 4194304,

    All = int.MaxValue
  }
}
