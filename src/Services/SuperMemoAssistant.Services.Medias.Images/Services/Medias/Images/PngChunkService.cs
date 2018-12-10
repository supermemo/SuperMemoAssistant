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
// Created On:   2018/06/03 01:42
// Modified On:  2018/06/03 02:32
// Modified By:  Alexis

#endregion




using System;
using System.IO;
using Hjg.Pngcs;
using Hjg.Pngcs.Chunks;

namespace SuperMemoAssistant.Services.Medias.Images
{
  public static class PngChunkService
  {
    #region Methods

    /// <summary>
    ///   If <paramref name="outFilePath" /> is NULL then file is written in-place.
    ///   <paramref name="chunkId" /> must follow the PNG conventions: four ascii letters, ID[0] :
    ///   lowercase (ancillary) ID[1] : lowercase if private, upppecase if public ID[3] : uppercase if
    ///   "safe to copy"
    /// </summary>
    public static void WriteCustomChunk(string inFilePath, string outFilePath, string chunkId, byte[] data)
    {
      PngChunk CreateChunk(ImageInfo imgInfo)
      {
        PngChunkUNKNOWN chunk = new PngChunkUNKNOWN(chunkId, imgInfo);
        chunk.SetData(data);
        chunk.Priority = true;

        return chunk;
      }

      using (var inStream = OpenInputStream(inFilePath, outFilePath == null))
        WriteChunk(inStream, outFilePath ?? inFilePath, CreateChunk);
    }

    /// <summary>If <paramref name="outFilePath" /> is NULL then file is written in-place.</summary>
    //public static void WriteZTxtChunk(string inFilePath, string outFilePath, string text)
    //{
    //  PngChunk CreateChunk(ImageInfo imgInfo)
    //  {
    //    PngChunkZTXT chunk = new PngChunkZTXT(imgInfo);
    //    chunk.SetKeyVal(data);
    //    chunk.Priority = true;

    //    return chunk;
    //  }

    //  using (var inStream = OpenInputStream(inFilePath, outFilePath == null))
    //    WriteChunk(inStream, outFilePath ?? inFilePath, CreateChunk);
    //}

    private static void WriteChunk(Stream inStream, string outFilePath, Func<ImageInfo, PngChunk> chunkFunc)
    {
      PngReader pngr = new PngReader(inStream);
      PngWriter pngw = FileHelper.CreatePngWriter(outFilePath, pngr.ImgInfo, true);

      pngw.CopyChunksFirst(pngr, ChunkCopyBehaviour.COPY_ALL);
      pngw.GetChunksList().Queue(chunkFunc(pngw.ImgInfo));

      for (int row = 0; row < pngr.ImgInfo.Rows; row++)
      {
        ImageLine l1 = pngr.ReadRow(row);
        pngw.WriteRow(l1, row);
      }

      pngw.CopyChunksLast(pngr, ChunkCopyBehaviour.COPY_ALL);

      pngr.End();
      pngw.End();
    }

    private static Stream OpenInputStream(string filePath, bool inMemory)
    {
      return inMemory
        ? ToMemoryStream(filePath)
        : File.OpenRead(filePath);
    }

    private static Stream ToMemoryStream(string filePath)
    {
      MemoryStream memStream = new MemoryStream();

      using (Stream inStream = File.OpenRead(filePath))
      {
        byte[] buffer = new byte[8192];

        while (inStream.Read(buffer, 0, buffer.Length) > 0)
          memStream.Write(buffer, 0, buffer.Length);
      }

      memStream.Seek(0, SeekOrigin.Begin);

      return memStream;
    }

    /// <summary>
    ///   <paramref name="chunkId" /> must follow the PNG conventions: four ascii letters,
    ///   ID[0] : lowercase (ancillary) ID[1] : lowercase if private, upppecase if public ID[3] :
    ///   uppercase if "safe to copy"
    /// </summary>
    public static byte[] ReadCustomChunk(string filePath, string chunkId)
    {
      return ReadChunk<byte[], PngChunkUNKNOWN>(filePath, chunkId, c => c?.GetData());
    }

#if false
    /// <summary>
    /// iTXt International textual data
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string ReadITxtChunk(string filePath)
    {
      return ReadChunk<string, PngChunkITXT>(filePath, PngChunkITXT.ID, c => c?.GetVal());
    }

    /// <summary>
    /// zTXt Compressed textual data
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string ReadZTxtChunk(string filePath)
    {
      return ReadChunk<string, PngChunkZTXT>(filePath, PngChunkZTXT.ID, c => c?.GetVal());
    }

    /// <summary>tEXt Textual data</summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string ReadTxtChunk(string filePath)
    {
      return ReadChunk<string, PngChunkTEXT>(filePath, PngChunkTEXT.ID, c => c?.GetVal());
    }
#endif

    private static TRet ReadChunk<TRet, TChunk>(string filePath, string chunkId, Func<TChunk, TRet> valueFunc)
      where TChunk : PngChunk
    {
      PngReader pngr = null;

      try
      {
        pngr = FileHelper.CreatePngReader(filePath);

        TChunk chunk = (TChunk)pngr.GetChunksList().GetById1(chunkId);

        return valueFunc(chunk);
      }
      finally
      {
        pngr?.End();
      }
    }

    #endregion
  }
}
