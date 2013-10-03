using System;

namespace Arbor.Sorbus.Core
{
    public sealed class AssemblyPatcherArgsParser
    {
        public AssemblyPatcherArgs Parse(string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }

            if (args.Length == 0)
            {
                throw new ArgumentException("A least on argument must be supplied");
            }

            if (args.Length == 1)
            {
                return null;
            }

            if (args.Length < 3)
            {
                throw new ArgumentException("Both Assembly Version and Assembly File Version must be entered");
            }

            return new AssemblyPatcherArgs
                       {
                           AssemblyVersion = new AssemblyVersion(Version.Parse(args[1])),
                           AssemblyFileVersion = new AssemblyFileVersion(Version.Parse(args[2]))
                       };
        }
    }
}