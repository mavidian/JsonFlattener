using System;
using System.Collections.Generic;
using System.IO;

namespace JsonFlattener
{
   public class JsonProducer : IDisposable
   {
      private StreamWriter _textWriter;

      public JsonProducer(StreamWriter textWriter)
      {
         _textWriter = textWriter;
      }


      public IEnumerable<object> PresentJsonTokenData(object setOfKvps)
      {
         throw new NotImplementedException();
      }


      public void WriteDataAsJson(object setOfKvps)
      {
         throw new NotImplementedException();
      }


      public void Dispose()
      {
         throw new NotImplementedException();
      }
   }
}
