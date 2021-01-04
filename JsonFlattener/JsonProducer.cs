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

      public JsonProducer(StreamWriter textWriter)
      {
         _jsonWriter = new JsonTextWriter(textWriter);
      }


      /// <summary>
      /// Convert a sequence of key-value pairs into corresponding JSON tokens (useful for testing/exploration purposes).
      /// </summary>
      /// <param name="items">Key-value pairs where Key reflects path to JSON element using special convention (undersore-delimited hierarchy of element names or array names with indeces). Represents a single record.</param>
      /// <returns>Sequence of resulting JSON tokens. </returns>
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
         foreach (var item in SortDataByKeyHierarchy(items))
         {  // items are sorted to assure intended nesting in JSON hierarchy
            if (isArray == null)
            {
               isArray = item.Key.StartsWith("[");
               if ((bool)isArray) _jsonWriter.WriteStartArray(); else _jsonWriter.WriteStartObject();
            }
            var segments = SplitColumnName(item.Key);
            var unchangedSegmentsCount = segments.Zip(prevKey.Reverse(), (f, s) => (Fst: f, Snd: s)).TakeWhile(t => t.Fst.Equals(t.Snd)).Count();
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
         if (isArray != null)  // null if empty items
         {
            if ((bool)isArray) _jsonWriter.WriteEndArray();
            else _jsonWriter.WriteEndObject();
         }
      }


      /// <summary>
      /// Sort data in a way to facilitate creation of nested JSON hierarchy.
      /// All top key segments are groupped together (in order of first appearance), then 2nd key segments are groupped together, etc.
      /// </summary>
      /// <param name="items">A sequence of key value pairs where key is a compound column name.</param>
      /// <returns>A sequence of sorted key value pairs.</returns>
      public IEnumerable<(string Key, object Value)> SortDataByKeyHierarchy(IEnumerable<(string Key, object Value)> items)
      {
         var wrappedItems = items.Select(i => (Wrapper: i.Key.Split('.','['), Item: i));  // Wrapper is an array of segments of the compound Key (note that array indices will contain closing brackets, but it doesn't matter)
         return SortWrappedData(wrappedItems).Select(wi => wi.Item);
      }


      /// <summary>
      /// Recursive method to sort wrapped items by the key segments.
      /// Each iteration groups items by the next-level key segments and outputs those items that have no key segments left.
      /// </summary>
      /// <param name="wrappedItems"></param>
      /// <returns></returns>
      private IEnumerable<(string[] Wrapper, (string Key, object Value) Item)> SortWrappedData(IEnumerable<(string[] Wrapper, (string Key, object Value) Item)> wrappedItems)
      {
         var grouppedItems = wrappedItems.GroupBy(wi => wi.Wrapper[0]);
         // Notice that segments are groupped in order of appearance. So, array elements are not sorted by index.
         foreach (var group in grouppedItems)
         {
            if (group.Count() == 1) yield return group.First();  // end of recursion (note that compound keys are assumed to be unique)
            else
            {  // remove top (groupped by) segment from the Wrapper when recursing
               var innerGroup = group.Select(wi => (Wrapper: wi.Wrapper.Skip(1).ToArray(), Item: wi.Item));
               foreach (var wi in SortWrappedData(innerGroup)) yield return wi;
            }
         }
      }


      /// <summary>
      /// Obtain a hierarchy of JSON elements from a compound column name (key).
      /// </summary>
      /// <param name="key">Compound column name.</param>
      /// <returns>A list of elements that the column name represents.</returns>
      private IEnumerable<LorC> SplitColumnName(string key)
      {
         var items = key.TrimStart('[').Split('.','[');  // array indices will contain closing brackets

         foreach (var item in items)
         {
            yield return char.IsDigit(item[0])
                        ? new LorC(int.Parse(item.TrimEnd(']')))  // counter
                        : new LorC(item);  // label
         }
      }


      public void Dispose()
      {
         ((IDisposable)_jsonWriter).Dispose();
      }
   }
}