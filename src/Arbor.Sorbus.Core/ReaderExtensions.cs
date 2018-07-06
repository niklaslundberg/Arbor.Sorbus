using System;
using System.IO;
using System.Text;

namespace Arbor.Sorbus.Core
{
    public static class ReaderExtensions
    {
         public static string ReadLineWithEol(this StreamReader streamReader)
         {
             if (streamReader == null)
             {
                 throw new ArgumentNullException(nameof(streamReader));
             }

             if (streamReader.Peek() < 0)
             {
                 return null;
             }

             var lineBuilder = new StringBuilder();

             while (true)
             {
                 if (streamReader.Peek() < 0)
                 {
                     break;
                 }

                 var next = streamReader.Read();
                 var twoForward = streamReader.Peek();

                 if (IsNewLineCharacter(next))
                 {
                     lineBuilder.Append((char) next);

                     if (IsLineFeedCharacter(twoForward))
                     {
                         lineBuilder.Append((char)twoForward);
                         streamReader.Read();
                     }

                     break;
                 }

                 lineBuilder.Append((char)next);
             }

             return lineBuilder.ToString();
         }

         static bool IsLineFeedCharacter(int next)
         {
             var c = ((char)next);
             return c == '\n';
        }

        static bool IsNewLineCharacter(int next)
        {
            var c = ((char) next);
            return c == '\r' || c == '\n';
        }
    }
}