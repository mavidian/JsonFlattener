using JsonFlattener;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace JsonProducer_tester
{
   class Program
   {
      private static readonly char sep = Path.DirectorySeparatorChar;

      private static readonly string _data = "TestData";
      //private static readonly string _data = "SampleMedicalEventData";

      private static readonly bool _outputToFile = true;  // if false, parsed segments of input are displayed one by one in the console window


      static void Main(string[] args)
      {
         var kvpInput = $"..{sep}..{sep}..{sep}Data{sep}{_data}.kvp";
         var jsonOutput = $"{_data}.json";

         Console.WriteLine($"KVP input file: {Path.GetFullPath(kvpInput)}");
         if (_outputToFile) Console.WriteLine($"JSON output file: {Path.GetFullPath(jsonOutput)}");

         using (var textReader = File.OpenText(kvpInput))
         using (var textWriter = File.CreateText(jsonOutput))
         using (var jsonProducer = new JsonProducer(textWriter))
         {
            var setOfKvps = GetLines(textReader).Select(l => ExtractKvp(l));  // key value pairs, each representing an item of columnar input (key = compound column name)

            if (_outputToFile)
            {
               jsonProducer.WriteDataAsJson(setOfKvps);
            }
            else  // step through input data
            {
               Console.WriteLine("Press a key to present next item..");
               foreach (var tokens in jsonProducer.PresentJsonTokenData(setOfKvps))
               {
                  Console.ReadKey(true);
                  Console.WriteLine(tokens);
               }
            }
         }

         if (Debugger.IsAttached)
         {
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey();
         }
      }


      /// <summary>
      /// Sequence of lines in the input file
      /// </summary>
      /// <param name="textReader"></param>
      /// <returns></returns>
      private static IEnumerable<string> GetLines(StreamReader textReader)
      {
         var line = textReader.ReadLine();
         while (line != null)
         {
            yield return line;
            line = textReader.ReadLine();
         }
      }

      private static Regex _kvpRegex = new Regex("Key=([^\\s]*)\\s+Value=(.*)");  // Key=(...)  Value=(...)
      /// <summary>
      /// Extract Key (compount column name) and Value from a single line.
      /// </summary>
      /// <param name="line"></param>
      /// <returns></returns>
      private static (string Key, object Value) ExtractKvp(string line)
      {
         //Each line is assumed to contain a single key value pair in the form: Key=...  Value=...
         var match = _kvpRegex.Match(line);
         return (match.Groups[1].Value, match.Groups[2].Value);
      }
   }
}
