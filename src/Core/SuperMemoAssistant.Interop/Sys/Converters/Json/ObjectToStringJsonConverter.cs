using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SuperMemoAssistant.Sys.Converters.Json
{
  /// <summary>
  /// Forces deserializing any valid json object as a string
  /// 
  /// https://stackoverflow.com/questions/29980580/deserialize-json-object-property-to-string
  /// </summary>
  public class JsonConverterObjectToString : JsonConverter
  {
    public override bool CanConvert(Type objectType)
    {
      return (objectType == typeof(JTokenType));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      JToken token = JToken.Load(reader);
      if (token.Type == JTokenType.Object)
      {
        return token.ToString();
      }
      return null;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      //serialize as actual JSON and not string data
      var token = JToken.Parse(value.ToString());

      writer.WriteToken(token.CreateReader());

    }
  }
}
