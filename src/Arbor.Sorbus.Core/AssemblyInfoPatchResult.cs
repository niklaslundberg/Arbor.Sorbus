namespace Arbor.Sorbus.Core
{
    public sealed class AssemblyInfoPatchResult
    {
        readonly AssemblyFileVersion _assemblyFileVersion;
        readonly AssemblyVersion _assemblyVersion;
        readonly string _fileBackupPath;
        readonly string _fullPath;
        readonly AssemblyMetaData _newAssemblyMetadata;
        readonly AssemblyFileVersion _oldAssemblyFileVersion;
        readonly AssemblyMetaData _oldAssemblyMetadata;
        readonly AssemblyVersion _oldAssemblyVersion;
        readonly bool _succeeded;

        public AssemblyInfoPatchResult(string fullPath,
            string fileBackupPath,
            AssemblyVersion oldAssemblyVersion,
            AssemblyVersion assemblyVersion,
            AssemblyFileVersion oldAssemblyFileVersion,
            AssemblyFileVersion assemblyFileVersion,
            bool succeeded = true,
            AssemblyMetaData oldAssemblyMetadata = null,
            AssemblyMetaData newAssemblyMetadata = null)
        {
            _fullPath = fullPath;
            _fileBackupPath = fileBackupPath;
            _oldAssemblyVersion = oldAssemblyVersion;
            _assemblyVersion = assemblyVersion;
            _oldAssemblyFileVersion = oldAssemblyFileVersion;
            _assemblyFileVersion = assemblyFileVersion;
            _succeeded = succeeded;
            _oldAssemblyMetadata = oldAssemblyMetadata;
            _newAssemblyMetadata = newAssemblyMetadata;
        }

        public AssemblyMetaData OldAssemblyMetadata
        {
            get { return _oldAssemblyMetadata; }
        }

        public AssemblyMetaData NewAssemblyMetadata
        {
            get { return _newAssemblyMetadata; }
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
                $"AssemblyFileVersion {AssemblyFileVersion}, AssemblyVersion {AssemblyVersion}, OldAssemblyFileVersion {OldAssemblyFileVersion}, OldAssemblyVersion {OldAssemblyVersion}, FileBackupPath {FileBackupPath}";
        }

        public static AssemblyInfoPatchResult Failed(string backupFile)
        {
            return new AssemblyInfoPatchResult("", backupFile, null, null, null, null, false);
        }
    }
}