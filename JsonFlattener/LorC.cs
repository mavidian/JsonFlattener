using System;

namespace JsonFlattener
{
   /// <summary>
   /// Label or Counter: "Either" type; type of elements in stack of prefixes. IOW, type of segments of compound column name.
   /// JSON object is represented on _keyPrefixes stack by Label; JSON array is represented by Label + Counter
   /// </summary>
   internal class LorC : IEquatable<LorC>
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
}
