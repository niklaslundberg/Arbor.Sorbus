namespace Arbor.Sorbus.Core
{
    public sealed class AssemblyPatcherArgs
    {
        public AssemblyVersion AssemblyVersion { get; set; }
        public AssemblyFileVersion AssemblyFileVersion { get; set; }
        public string SourceBase { get; set; }
    }
}