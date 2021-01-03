using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace JsonFlattener
{
   /// <summary>
   /// A reader of JSON stream and converter into a sequence of (flattened) key value pairs, where keys are coumpound names representing JSON element hierarchy.
   /// </summary>
   public class JsonConsumer : IDisposable
   {
      private readonly JsonTextReader _jsonReader;

      public JsonConsumer(TextReader textReader)
      {
         _jsonReader = new JsonTextReader(textReader);
      }


      /// <summary>
      /// Sequence of tokens obtained from JSON input (useful for testing/exploration purposes).
      /// </summary>
      /// <returns></returns>
      public IEnumerable<(JsonToken Type, object Value)> GetJsonTokenData()
      {
         while (_jsonReader.Read())
         {
            yield return (_jsonReader.TokenType, _jsonReader.Value?.ToString());
         }
      }


      /// <summary>
      /// Sequence of flattened key-value pairs obtained from JSON input.
      /// Key reflects the path to JSON element and uniquely defines each value.
      /// </summary>        
      /// <returns></returns>
      public IEnumerable<KeyValuePair<string, object>> GetFlattenedData()
      {
         while (_jsonReader.Read())
         {
            switch (_jsonReader.TokenType)
            {
               case JsonToken.String:
               case JsonToken.Integer:
               case JsonToken.Float:
               case JsonToken.Boolean:
               case JsonToken.Null:
               case JsonToken.Undefined:  // absence of value, e.g. "MyUndefinedValue":, - technically invalid JSON, but Json.NET parses it as Undefined
                  yield return new KeyValuePair<string, object>(_jsonReader.Path, _jsonReader.Value);
                  break;
            }
         }
      }


      public void Dispose()
      {
         ((IDisposable)_jsonReader).Dispose();
      }

   }
}
