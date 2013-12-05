using System.IO;
using System.Text;
using Arbor.Sorbus.Core;
using Machine.Specifications;
using Machine.Specifications.Model;

namespace Arbor.Sorbus.Tests.Integration
{
    [Subject(typeof (Subject))]
    public class when_reading_lines_with_crlf
    {
        static string text;
        static StreamReader reader;

        static MemoryStream stream;
        static string textCopied;

        Cleanup cleanup = () =>
        {
            using (stream)
            {
            }
        };

        Establish context = () =>
        {
            text = "abc\r\n123\r\ndef\r\nghi";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            reader = new StreamReader(stream);
        };

        Because of = () =>
        {
            StringBuilder builder = new StringBuilder();

            while (!reader.EndOfStream)
            {
                builder.Append(reader.ReadLineWithEol());
            }

            textCopied = builder.ToString();
        };

        It should_keep_the_crlf = () => textCopied.ShouldEqual(text);
    }
}