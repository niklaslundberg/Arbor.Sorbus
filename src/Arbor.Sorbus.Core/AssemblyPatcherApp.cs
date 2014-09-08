using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Arbor.Aesculus.Core;
using Newtonsoft.Json;

namespace Arbor.Sorbus.Core
{
    public sealed class AssemblyPatcherApp
    {
        public void Patch(AssemblyVersion assemblyVersion, AssemblyFileVersion assemblyFileVersion, string sourceBase)
        {
            var patcher = new AssemblyPatcher();

            string path = VcsPathHelper.FindVcsRootPath();

            IReadOnlyCollection<AssemblyInfoFile> assemblyInfoFiles =
                Directory.EnumerateFiles(path, "AssemblyInfo.cs", SearchOption.AllDirectories)
                    .Where(file =>
                        file.IndexOf(AssemblyPatcher.Patchedassemblyinfos,
                            StringComparison.InvariantCultureIgnoreCase) < 0)
                    .Select(file => new AssemblyInfoFile(file))
                    .ToReadOnly();

            PatchResult result = patcher.Patch(assemblyInfoFiles, assemblyVersion, assemblyFileVersion, sourceBase);

            patcher.SavePatchResult(result);
        }

        public void Unpatch(string sourceBase)
        {
            var patcher = new AssemblyPatcher();
            string resultFilePath = Path.Combine(patcher.BackupBasePath(), "Patched.txt");

            if (!File.Exists(resultFilePath))
            {
                Console.WriteLine("Could not find any file '{0}'. No files were patched.", resultFilePath);
                return;
            }

            string json = File.ReadAllText(resultFilePath, Encoding.UTF8);
            var deserialized = JsonConvert.DeserializeObject<List<AssemblyInfoPatchResult>>(json);

            var patchResult = new PatchResult();

            deserialized.ForEach(patchResult.Add);

            List<AssemblyInfoPatchResult> unpatched =
                patcher.Unpatch(patchResult, sourceBase)
                    .SelectMany(
                        results =>
                            results.Where(
                                result =>
                                    result.AssemblyVersion.Version != result.OldAssemblyVersion.Version ||
                                    result.AssemblyFileVersion.Version != result.OldAssemblyFileVersion.Version))
                    .ToList();

            string verb = unpatched.Count == 1 ? "was" : "were";

            Console.WriteLine("{0} items {1} patched", unpatched.Count, verb);

            foreach (AssemblyInfoPatchResult result in unpatched)
            {
                Console.WriteLine("{0} changed from version {1} to version {2}", result.FullPath,
                    result.OldAssemblyVersion, result.AssemblyVersion);
            }
        }
    }
}