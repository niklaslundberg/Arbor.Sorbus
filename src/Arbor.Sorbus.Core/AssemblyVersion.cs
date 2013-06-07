using System;

namespace Arbor.Sorbus.Core
{
    public class AssemblyVersion
    {
        readonly Version _version;

        public AssemblyVersion(Version version)
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
            return Version.ToString();
        }
    }
}