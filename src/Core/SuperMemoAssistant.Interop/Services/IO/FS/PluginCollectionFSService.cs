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
// Created On:   2018/06/02 00:38
// Modified On:  2018/06/02 00:40
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.IO;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Sys.IO.FS;

namespace SuperMemoAssistant.Services.IO.FS
{
  public class PluginCollectionFSService : ICollectionFSService
  {
    #region Properties & Fields - Non-Public

    private ICollectionFSService Service { get; }

    private ISMAPlugin Plugin { get; }

    #endregion




    #region Constructors

    public PluginCollectionFSService(ISMAPlugin plugin, ICollectionFSService service)
    {
      Plugin  = plugin;
      Service = service;
    }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public CollectionFile this[int fileId] => Service[fileId];

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public IEnumerable<CollectionFile> ForElement(int elementId, ISMAPlugin plugin = null)
    {
      return Service.ForElement(elementId, plugin);
    }
    
    public IEnumerable<CollectionFile> ForElementWithPlugin(int elementId)
    {
      return Service.ForElement(elementId, Plugin);
    }

    /// <inheritdoc />
    public IEnumerable<CollectionFile> ForPlugin(ISMAPlugin plugin = null)
    {
      return Service.ForPlugin(plugin ?? Plugin);
    }

    /// <inheritdoc />
    public CollectionFile Create(ISMAPlugin plugin, int elementId, Action<Stream> streamWriter, string extension, string crc32 = null)
    {
      return Service.Create(plugin, elementId, streamWriter, extension, crc32);
    }
    
    public CollectionFile Create(int elementId, Action<Stream> streamWriter, string extension, string crc32 = null)
    {
      return Service.Create(Plugin, elementId, streamWriter, extension, crc32);
    }

    /// <inheritdoc />
    public bool DeleteById(int fileId)
    {
      return Service.DeleteById(fileId);
    }

    /// <inheritdoc />
    public int DeleteByElementId(int elementId, ISMAPlugin plugin = null)
    {
      return Service.DeleteByElementId(elementId, plugin);
    }
    
    public int DeleteByElementIdWithPlugin(int elementId)
    {
      return Service.DeleteByElementId(elementId, Plugin);
    }

    /// <inheritdoc />
    public int DeleteByPlugin(ISMAPlugin plugin = null)
    {
      return Service.DeleteByPlugin(plugin ?? Plugin);
    }

    #endregion
  }
}
