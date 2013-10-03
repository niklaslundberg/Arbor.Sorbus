namespace Arbor.Sorbus.Core
{
    public sealed class AssemblyInfoPatchResult
    {
        readonly AssemblyFileVersion _assemblyFileVersion;
        readonly AssemblyVersion _assemblyVersion;
        readonly string _fileBackupPath;
        readonly string _fullPath;
        readonly AssemblyFileVersion _oldAssemblyFileVersion;
        readonly AssemblyVersion _oldAssemblyVersion;

        public AssemblyInfoPatchResult(string fullPath, string fileBackupPath, AssemblyVersion oldAssemblyVersion,
                                       AssemblyVersion assemblyVersion, AssemblyFileVersion oldAssemblyFileVersion,
                                       AssemblyFileVersion assemblyFileVersion)
        {
            _fullPath = fullPath;
            _fileBackupPath = fileBackupPath;
            _oldAssemblyVersion = oldAssemblyVersion;
            _assemblyVersion = assemblyVersion;
            _oldAssemblyFileVersion = oldAssemblyFileVersion;
            _assemblyFileVersion = assemblyFileVersion;
        }

        public AssemblyFileVersion AssemblyFileVersion
        {
            get { return _assemblyFileVersion; }
        }

        public AssemblyVersion AssemblyVersion
        {
            get { return _assemblyVersion; }
        }

        public string FileBackupPath
        {
            get { return _fileBackupPath; }
        }

        public string FullPath
        {
            get { return _fullPath; }
        }

        public AssemblyFileVersion OldAssemblyFileVersion
        {
            get { return _oldAssemblyFileVersion; }
        }

        public AssemblyVersion OldAssemblyVersion
        {
            get { return _oldAssemblyVersion; }
        }
    }
}