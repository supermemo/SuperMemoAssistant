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
// Modified On:  2020/02/02 23:02
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Threading.Tasks;
using SuperMemoAssistant.Extensions;

namespace SuperMemoAssistant.Sys.Windows.Search
{
  /// <summary>
  /// https://github.com/shellscape/Lumen/blob/master/Application/Search/WindowsSearchProvider.cs
  /// </summary>
  public class WindowsSearch
  {
    #region Constants & Statics

    // Shared connection used for any search index queries

    private readonly OleDbConnection _connection;

    /// <summary>Indicates if Winders Desktop Search is installed and enabled.</summary>
    public bool IsAvailable => _connection != null;

    #endregion




    public static WindowsSearch Instance { get; } = new WindowsSearch();




    #region Constructors

    protected WindowsSearch()
    {
      try
      {
        _connection = new OleDbConnection("Provider=Search.CollatorDSO;Extended Properties='Application=Windows';");
        _connection.Open();
      }
      catch
      {
        // fails if desktop search is disabled or not installed
        _connection = null;
      }
    }

    #endregion




    #region Methods

    public async Task<List<WindowsSearchResult>> Search(
      string term,
      WindowsSearchKind kind = WindowsSearchKind.All,
      int limit = 1000)
    {
      var list = new List<WindowsSearchResult>();

      // prevent hidden (0x2) and system (0x4) files
      var sql = @$"select top {limit} 
        System.ItemNameDisplay, System.ItemPathDisplay, System.Kind, System.Search.Rank, System.FileAttributes 
        from systemindex 
        where CONTAINS(""System.ItemNameDisplay"", '""*{term.Trim()}*""')";

      if (kind != WindowsSearchKind.All)
        sql += $"and System.Kind = '{kind.ToString().ToLower()}'";
      
      sql += @"and System.FileAttributes <> ALL BITWISE 0x4 and System.FileAttributes <> ALL BITWISE 0x2
        order by System.Search.Rank";

      using (OleDbCommand command = new OleDbCommand())
      {
        command.Connection  = _connection;
        command.CommandText = sql;

        using (var reader = await command.ExecuteReaderAsync())
          while (await reader.ReadAsync())
          {
            var result = new WindowsSearchResult()
            {
              FileName = reader["System.ItemNameDisplay"].ToString(),
              FilePath = reader["System.ItemPathDisplay"].ToString(),
              Rank     = (int)reader["System.Search.Rank"]
            };

            if (reader["System.Kind"] is string[] kinds && kinds.Length >= 1)
              foreach (var k in reader["System.Kind"] as string[])
                result.Kind |= (WindowsSearchKind)Enum.Parse(typeof(WindowsSearchKind), k, true);

            else
              result.Kind = WindowsSearchKind.File;

            list.Add(result);
          }
      }

      var results = list.DistinctBy(p => p.FileName).ToList();

      return results;
    }

    /// <summary>
    ///   Returns values from the search index for the given filename based on the supplied
    ///   list of columns/properties
    /// </summary>
    /// <param name="columns">
    ///   Comma-separated list of columns/properties
    ///   (http://msdn2.microsoft.com/en-us/library/ms788673.aspx)
    /// </param>
    /// <param name="filename">Filename about which to retrieve data</param>
    /// <returns>Key/value pairs for all requested columns/properties</returns>
    /// <remarks></remarks>
    public Dictionary<string, string> GetProperties(string columns, string filename)
    {
      Dictionary<string, string> items = new Dictionary<string, string>();

      // The search provider does not support SQL parameters
      filename = filename.Replace("'", "''");

      OleDbCommand cmd = new OleDbCommand
      {
        Connection = _connection, CommandText = $"SELECT {columns} from systemindex WHERE System.ItemNameDisplay = '{filename}'"
      };

      OleDbDataReader results = cmd.ExecuteReader();

      if (results.Read())
      {
        int i = 0;

        while (i < results.FieldCount)
        {
          if (!results.IsDBNull(i))
          {
            var    key = results.GetName(i);
            string value;

            if (results.GetValue(i) is string[])
            {
              string[] arrayValue = results.GetValue(i) as string[];

              value = arrayValue.GetLength(0) == 1 ? arrayValue[0] : string.Join(", ", arrayValue);
            }
            else
            {
              value = results.GetValue(i).ToString();
            }

            if (key.StartsWith("System."))
              key = key.Substring(7);
            items.Add(key, value);
          }

          i = i + 1;
        }
      }

      results.Close();

      return items;
    }

    #endregion
  }
}
