using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SuperMemoAssistant.Services.HTML
{
  public static class CookiesUtils
  {
    /// <summary>
    /// Parse a cookies string and return individual values
    /// Original : https://github.com/NancyFx/Nancy/blob/master/src/Nancy/Request.cs
    /// </summary>
    /// <param name="inlineCookies">Cookies string</param>
    /// <param name="urlEncode">Escapes invalid url characters</param>
    /// <returns>Name-value pairs</returns>
    public static IDictionary<string, string> ParseCookies(string inlineCookies, bool urlEncode = false)
    {
      var cookieDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
      var values = inlineCookies.TrimEnd(';').Split(';');

      foreach (var parts in values.Select(c => c.Split(new[] { '=' }, 2)))
      {
        var cookieName = parts[0].Trim();
        var cookieValue = parts.Length == 1 ?
          string.Empty
          : HttpUtility.UrlDecode(parts[1]);

        if (urlEncode)
          cookieValue = HttpUtility.UrlEncode(cookieValue);

        cookieDictionary[cookieName] = cookieValue;
      }

      return cookieDictionary;
    }
  }
}
