using System;

namespace Arbor.Sorbus.Core
{
    public sealed class AssemblyFileVersion
    {
        readonly Version _version;

        public AssemblyFileVersion(Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            _version = version;
        }

        public Version Version
        {
            get { return _version; }
        }

        public override string ToString()
        {
            return _version.ToString();
        }
    }
}