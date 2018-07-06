using System;

namespace Arbor.Sorbus.Core
{
    public sealed class AssemblyVersion
    {
        readonly Version _version;

        public AssemblyVersion(Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            _version = version;
        }

        public Version Version
        {
            get { return _version; }
        }

        public override string ToString()
        {
            return Version.ToString();
        }
    }
}