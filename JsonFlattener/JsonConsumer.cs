using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace JsonFlattener
{
   /// <summary>
   /// A reader of JSON stream and converter into a sequence of (flattened) key value pairs, where keys are coumpound names representing JSON element hierarchy.
   /// </summary>
   public class JsonConsumer : IDisposable
   {
      private readonly JsonTextReader _jsonReader;

      private readonly Stack<LorC> _keyPrefixes;  // elements to build column name from
      private const string _pfxSep = "_";  // separates prefixes, i.e. segments of column nams
      private string _LPN;  // last PropertyName
      private readonly LorC _emptyLabel = new LorC(string.Empty);  // part to ignore when building columns name from _keyPrefixes

      public JsonConsumer(TextReader textReader)
      {
         _jsonReader = new JsonTextReader(textReader);
         _keyPrefixes = new Stack<LorC>();
         _keyPrefixes.Push(_emptyLabel);
         _LPN = string.Empty;
         // If JSON contains top-level array (i.e. [...]), then compound column names will start at numbers; to prevent this
         // _LPN can be assinged here a value, such as "ROOT".
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
            var tokenType = _jsonReader.TokenType;
            var tokenValue = _jsonReader.Value;

            switch (tokenType)
            {
               case JsonToken.StartArray:
                  PushLPNorEmptyLabelIfCounterOnTop();
                  _keyPrefixes.Push(new LorC()); //counter
                  break;
               case JsonToken.StartObject:
                  PushLPNorEmptyLabelIfCounterOnTop();
                  break;
               case JsonToken.PropertyName:
                  _LPN = (string)tokenValue;  // remember last PropertyName value for use in the next token
                  break;
               case JsonToken.String:
               case JsonToken.Integer:
               case JsonToken.Float:
               case JsonToken.Boolean:
               case JsonToken.Null:
               case JsonToken.Undefined:  // absence of value, e.g. "MyUndefinedValue":, - technically invalid JSON, but Json.NET parses it as Undefined
                  yield return new KeyValuePair<string, object>(GetColumnName(), tokenValue);
                  IncrementCounterIfAnyOnTop();
                  break;
               case JsonToken.EndObject:
                  Debug.Assert(_keyPrefixes.Peek().IsLabel);
                  _keyPrefixes.Pop();
                  IncrementCounterIfAnyOnTop();
                  break;
               case JsonToken.EndArray:
                  Debug.Assert(_keyPrefixes.Peek().IsCounter);
                  _keyPrefixes.Pop();
                  Debug.Assert(_keyPrefixes.Peek().IsLabel);
                  _keyPrefixes.Pop();
                  IncrementCounterIfAnyOnTop();
                  break;
               default:
                  throw new InvalidDataException($"Unexpected JSON '{tokenType}' token with a value '{tokenValue}'.");
            }
         }
      }


      private void PushLPNorEmptyLabelIfCounterOnTop()
      {
         //Counter on top means that array is the last element on _keyPrefixes stack. If so, the current element will not become part of the Key - note that
         // array elements are not named. However,  an emptyLabel element still needs to be added to properly track object/array nesting.
         _keyPrefixes.Push(_keyPrefixes.Peek().IsLabel ? new LorC(_LPN) : _emptyLabel);
      }


      private string GetColumnName()
      {
         // Empty labels are elememnts on the _keyPrefixes stack that are not part of the key hierarchy (such as those elements that immediately follow counters, i.e. JSON arrays).
         return string.Join(_pfxSep, _keyPrefixes.Reverse()
                                              .Append(_keyPrefixes.Peek().IsLabel ? new LorC(_LPN) : _emptyLabel)
                                              .Select(p => p.Value)
                                              .Where(v => v != string.Empty));
      }


      private void IncrementCounterIfAnyOnTop()
      {
         var topPfx = _keyPrefixes.Peek();
         if (topPfx.IsCounter) topPfx.Increment();
      }

      public void Dispose()
      {
         ((IDisposable)_jsonReader).Dispose();
      }


   }
}
