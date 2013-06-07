using System;
using Arbor.Sorbus.Core;

namespace Arbor.Sorbus.ConsoleApp
{
    internal class Program
    {
        static int Main(string[] args)
        {
            int result;
            try
            {
                var app = new AssemblyPatcherApp();

                app.PatchOrUnpatch(args);

                result = 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                result = -1;
            }

            return result;
        }
    }
}