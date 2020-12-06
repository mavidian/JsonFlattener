using JsonFlattener;
using System;
using System.Diagnostics;
using System.IO;

namespace JsonConsumer_tester
{
   class Program
   {
      private static readonly char sep = Path.DirectorySeparatorChar;

      private static readonly string _data = "TestData";
      //private static readonly string _data = "SampleMedicalEventData";

      private static readonly bool _outputToFile = true;  // if false, detailed JSON tokens are displayed one by one on the console window

 
      static void Main(string[] args)
      {
         var jsonInput = $"..{sep}..{sep}..{sep}Data{sep}{_data}.json";
         var kvpOutput = $"{_data}.kvp";

         Console.WriteLine($"JSON input file: {Path.GetFullPath(jsonInput)}");
         if (_outputToFile) Console.WriteLine($"KVP output file: {Path.GetFullPath(kvpOutput)}");

         using (var textReader = File.OpenText(jsonInput))
         using (var jsonConsumer = new JsonConsumer(textReader))
         using (var textWriter = File.CreateText(kvpOutput))
         {

            if (_outputToFile)
            {
               foreach (var kvp in jsonConsumer.GetFlattenedData())
               {
                  textWriter.WriteLine($"Key={kvp.Key}  Value={kvp.Value}");
               }
            }
            else // step through generated JSON tokens
            {
               Console.WriteLine("Press a key to obtain next token..");
               foreach (var token in jsonConsumer.GetJsonTokenData())
               {
                  Console.ReadKey(true);
                  Console.WriteLine($"  {token.Type,-16}{(token.Value == null ? string.Empty : token.Value)}");
               }
               Console.WriteLine("End of JSON data.");
            }
         }

         if (Debugger.IsAttached)
         {
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey();
         }

      }
   }
}
