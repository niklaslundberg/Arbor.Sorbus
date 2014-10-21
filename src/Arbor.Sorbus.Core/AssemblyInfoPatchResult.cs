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
        bool _succeeded;

        public AssemblyInfoPatchResult(string fullPath, string fileBackupPath, AssemblyVersion oldAssemblyVersion,
            AssemblyVersion assemblyVersion, AssemblyFileVersion oldAssemblyFileVersion,
            AssemblyFileVersion assemblyFileVersion, bool succeeded = true)
        {
            _fullPath = fullPath;
            _fileBackupPath = fileBackupPath;
            _oldAssemblyVersion = oldAssemblyVersion;
            _assemblyVersion = assemblyVersion;
            _oldAssemblyFileVersion = oldAssemblyFileVersion;
            _assemblyFileVersion = assemblyFileVersion;
            _succeeded = succeeded;
        }

        public bool Succeeded
        {
            get { return _succeeded; }
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

        public override string ToString()
        {
            return
                string.Format(
                    "AssemblyFileVersion {0}, AssemblyVersion {1}, OldAssemblyFileVersion {2}, OldAssemblyVersion {3}, FileBackupPath {4}",
                    AssemblyFileVersion, AssemblyVersion, OldAssemblyFileVersion, OldAssemblyVersion, FileBackupPath);
        }

        public static AssemblyInfoPatchResult Failed(string backupFile)
        {
            return new AssemblyInfoPatchResult("", backupFile, null, null, null, null, false);
        }
    }
}