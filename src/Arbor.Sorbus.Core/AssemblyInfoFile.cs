namespace Arbor.Sorbus.Core
{
    public class AssemblyInfoFile
    {
        readonly string _assemblyInfoPath;

        public AssemblyInfoFile(string assemblyInfoPath)
        {
            _assemblyInfoPath = assemblyInfoPath;
        }

        public string FullPath
        {
            get { return _assemblyInfoPath; }
        }
    }
}