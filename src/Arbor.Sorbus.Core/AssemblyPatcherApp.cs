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
        public void PatchOrUnpatch(string[] args)
        {
            var parser = new AssemblyPatcherArgsParser();
            var patcherArgs = parser.Parse(args);

            if (patcherArgs == null)
            {
                Unpatch();
            }
            else
            {
                Patch(patcherArgs.AssemblyVersion, patcherArgs.AssemblyFileVersion);
            }
        }

        public void Patch(AssemblyVersion assemblyVersion, AssemblyFileVersion assemblyFileVersion)
        {
            var patcher = new AssemblyPatcher();

            var path = VcsPathHelper.FindVcsRootPath();

            var assemblyInfoFiles = Directory.EnumerateFiles(path, "AssemblyInfo.cs", SearchOption.AllDirectories)
                                             .Where(file =>
                                                    file.IndexOf(AssemblyPatcher.Patchedassemblyinfos,
                                                                 StringComparison.InvariantCultureIgnoreCase) < 0)
                                             .Select(file => new AssemblyInfoFile(file))
                                             .ToReadOnly();

            var result = patcher.Patch(assemblyInfoFiles, assemblyVersion, assemblyFileVersion);

            patcher.SavePatchResult(result);
        }

        public void Unpatch()
        {
            var patcher = new AssemblyPatcher();
            var resultFilePath = System.IO.Path.Combine(patcher.BackupBaseDirectory.FullName, "Patched.txt");

            if (!File.Exists(resultFilePath))
            {
                Console.WriteLine("Could not find any file '{0}'. No files were patched.", resultFilePath);
                return;
            }

            var json = File.ReadAllText(resultFilePath, Encoding.UTF8);
            var deserialized = JsonConvert.DeserializeObject<List<AssemblyInfoPatchResult>>(json);

            var patchResult = new PatchResult();

            deserialized.ForEach(patchResult.Add);

            var unpatched =
                patcher.Unpatch(patchResult)
                       .SelectMany(
                           results =>
                           results.Where(
                               result =>
                               result.AssemblyVersion.Version != result.OldAssemblyVersion.Version ||
                               result.AssemblyFileVersion.Version != result.OldAssemblyFileVersion.Version))
                       .ToList();

            var verb = unpatched.Count == 1 ? "was" : "were";

            Console.WriteLine("{0} items {1} patched", unpatched.Count, verb);

            foreach (var result in unpatched)
            {
                Console.WriteLine("{0} changed from version {1} to version {2}", result.FullPath,
                                  result.OldAssemblyVersion, result.AssemblyVersion);
            }
        }
    }
}