using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JsonFlattener
{

   /// <summary>
   /// A JSON writer and converter from a sequence of key value pairs, where keys are compound names representing element hierarchy in JSON. 
   /// </summary>
   public class JsonProducer : IDisposable
   {
      private JsonTextWriter _jsonWriter;

      /// <summary>
      /// Label or Counter: "Either" type; type of elements in stack of prefixes. IOW, type of segments of compound column name.
      /// JSON object is represented on _keyPrefixes stack by Label; JSON array is represented by Label + Counter
      /// </summary>
      private class LorC : IEquatable<LorC>
      {
         private string _label;
         private int _counter;

         public LorC(string label) { _label = label; }  // creates Label
         public LorC() { _label = null; _counter = 1; }  // creates Counter (1-based)
         public LorC(int counter) { _label = null; _counter = counter; }  // creates Counter from existing segment of column name

         public bool IsCounter => _label == null;
         public bool IsLabel => !IsCounter;

         public string Label => IsLabel ? _label : string.Empty;
         public int Counter => IsCounter ? _counter : default;
         public string Value => IsLabel ? _label : $"{_counter:D2}";

         public void Increment() => _counter++;

         public bool Equals(LorC other)
         {
            if (other == null) return false;
            return Label == other.Label && Counter == other.Counter;
         }
      }


      public JsonProducer(StreamWriter textWriter)
      {
         _jsonWriter = new JsonTextWriter(textWriter);
      }


      /// <summary>
      /// Convert a sequence of key-value pairs into corresponding JSON tokens (useful for testing/exploration purposes).
      /// </summary>
      /// <param name="items">Key-value pairs where Key reflects path to JSON element using special convention (undersore-delimited hierarchy of element names or array names with indeces). Represents a single record.</param>
      /// <returns>Sequence of resulting JSON tokens. </returns>
      ////internal IEnumerable<(JsonToken Type, object Value)> PresentJsonTokenData(IEnumerable<(string Key, object Value)> items)
      public IEnumerable<string> PresentJsonTokenData(IEnumerable<(string Key, object Value)> items)
      {
         foreach (var item in items)
         {
            var segments = SplitColumnName(item.Key);
            yield return segments.Aggregate(string.Empty, (a, s) => a + (s.IsLabel ? $" {s.Label}" : $"({s.Counter})")) + $" = {item.Value}";
         }
      }


      /// <summary>
      /// Send a sequence of key-value pairs as a JSON hierarchy to a text writer.
      /// </summary>
      /// <param name="items">Key-value pairs where Key reflects path to JSON element using special convention (undersore-delimited hierarchy of element names or array names with indeces). Represents a single record.</param>
      public void WriteDataAsJson(IEnumerable<(string Key, object Value)> items)
      {
         bool? isArray = null;
         var prevKey = new Stack<LorC>();
         prevKey.Push(new LorC("dummy")); //will get removed
         foreach (var item in items)
         {
            if (isArray == null)
            {
               isArray = char.IsDigit(item.Key.First());
               if ((bool)isArray) _jsonWriter.WriteStartArray(); else _jsonWriter.WriteStartObject();
            }
            var segments = SplitColumnName(item.Key);
            var unchangedSegmentsCount = segments.Zip(prevKey.Reverse(), (f,s) => (Fst:f,Snd:s)).TakeWhile(t => t.Fst.Equals(t.Snd)).Count();
            while (prevKey.Count > unchangedSegmentsCount + 1)
            {
               var segment = prevKey.Pop();
               if (segment.IsLabel) _jsonWriter.WriteEndObject(); else _jsonWriter.WriteEndArray();
            }
            prevKey.Pop();

            bool first = true;
            foreach (var segment in segments.Skip(unchangedSegmentsCount))
            {
               if (segment.IsLabel)
               {
                  if (!first) _jsonWriter.WriteStartObject();
                  _jsonWriter.WritePropertyName(segment.Label);
               }
               else //IsCounter
               {
                  if (!first) _jsonWriter.WriteStartArray();
               }
               first = false;
               prevKey.Push(segment);
            }
            _jsonWriter.WriteValue(item.Value);
         }
         if ((bool)isArray) _jsonWriter.WriteEndArray(); else _jsonWriter.WriteEndObject();
      }


      /// <summary>
      /// Obtain a hierarchy of JSON elements from a compound column name (key).
      /// </summary>
      /// <param name="key">Compound column name.</param>
      /// <returns>A list of elements that the column name represents.</returns>
      private IEnumerable<LorC> SplitColumnName(string key)
      {
         var items = key.Split('_');

         foreach (var item in items)
         {
            yield return char.IsDigit(item[0])
                        ? new LorC(int.Parse(item))  // counter
                        : new LorC(item);  // label
         }
      }

      public void Dispose()
      {
         ((IDisposable)_jsonWriter).Dispose();
      }
   }
}