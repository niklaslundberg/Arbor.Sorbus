﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Arbor.Sorbus.Core
{
    public sealed class AssemblyPatcherApp
    {
        private readonly Action<string> _logger;

        public AssemblyPatcherApp(Action<string> logger = null)
        {
            _logger = logger;
        }

        public void Patch(
            AssemblyVersion assemblyVersion,
            AssemblyFileVersion assemblyFileVersion,
            string sourceBase,
            string assemblyfilePattern = "AssemblyInfo.cs",
            AssemblyMetaData assemblyMetaData = null)
        {
            if (assemblyVersion == null)
            {
                throw new ArgumentNullException(nameof(assemblyVersion));
            }

            if (assemblyFileVersion == null)
            {
                throw new ArgumentNullException(nameof(assemblyFileVersion));
            }

            if (sourceBase == null)
            {
                throw new ArgumentNullException(nameof(sourceBase));
            }

            var patcher = new AssemblyPatcher(sourceBase, _logger);

            IReadOnlyCollection<AssemblyInfoFile> assemblyInfoFiles =
                Directory.EnumerateFiles(sourceBase, assemblyfilePattern, SearchOption.AllDirectories)
                    .Where(file =>
                        file.IndexOf(patcher.PatchedassemblyinfosPath,
                            StringComparison.InvariantCultureIgnoreCase) < 0)
                    .Where(
                        file =>
                            file.IndexOf(string.Format("{0}packages{0}", Path.DirectorySeparatorChar),
                                StringComparison.InvariantCultureIgnoreCase) < 0)
                    .Select(file => new AssemblyInfoFile(file))
                    .ToReadOnly();

            Patch(assemblyVersion, assemblyFileVersion, sourceBase, assemblyInfoFiles, assemblyMetaData);
        }

        public void Patch(
            AssemblyVersion assemblyVersion,
            AssemblyFileVersion assemblyFileVersion,
            string sourceBase,
            IEnumerable<AssemblyInfoFile> assemblyInfoFiles,
            AssemblyMetaData assemblyMetaData = null)
        {
            if (assemblyVersion == null)
            {
                throw new ArgumentNullException(nameof(assemblyVersion));
            }

            if (assemblyFileVersion == null)
            {
                throw new ArgumentNullException(nameof(assemblyFileVersion));
            }

            if (sourceBase == null)
            {
                throw new ArgumentNullException(nameof(sourceBase));
            }

            if (assemblyInfoFiles == null)
            {
                throw new ArgumentNullException(nameof(assemblyInfoFiles));
            }

            var patcher = new AssemblyPatcher(sourceBase, _logger);

            PatchResult result = patcher.Patch(assemblyInfoFiles.ToList(),
                assemblyVersion,
                assemblyFileVersion,
                assemblyMetaData);

            patcher.SavePatchResult(result);
        }

        public void Unpatch(string sourceBase)
        {
            var patcher = new AssemblyPatcher(sourceBase, _logger);
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
                patcher.Unpatch(patchResult)
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
                Console.WriteLine("{0} changed from version {1} to version {2}",
                    result.FullPath,
                    result.OldAssemblyVersion,
                    result.AssemblyVersion);
            }
        }
    }
}