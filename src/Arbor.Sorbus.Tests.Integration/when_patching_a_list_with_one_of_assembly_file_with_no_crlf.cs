using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arbor.Sorbus.Core;
using Machine.Specifications;

namespace Arbor.Sorbus.Tests.Integration
{
    [Subject(typeof(AssemblyPatcher))]
    public class when_patching_a_list_with_one_of_assembly_file_with_no_crlf : patch_assembly_info_base_no_crlf
    {
        static AssemblyPatcher assemblyPatcher;
        static IEnumerable<AssemblyInfoFile> assemblyInfoFiles;
        static AssemblyVersion assemblyVersion;
        static AssemblyFileVersion assemblyFileVersion;
        static PatchResult patchResult;

        Establish context = () =>
        {
            assemblyPatcher = new AssemblyPatcher();
            var assemblyInfoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AssemblyInfoMissingCRLF.cs");

            assemblyInfoFiles = new List<AssemblyInfoFile>
                                {
                                    new AssemblyInfoFile(assemblyInfoPath)
                                };
            assemblyVersion = new AssemblyVersion(new Version(2, 3, 0, 0));
            assemblyFileVersion = new AssemblyFileVersion(new Version(2, 3, 4, 5));

            patchResult = assemblyPatcher.Patch(assemblyInfoFiles.ToReadOnly(),
                new AssemblyVersion(new Version(1, 2, 0, 0)),
                new AssemblyFileVersion(new Version(1, 2, 3, 4)));
        };

        Because of =
            () =>
            {
                patchResult = assemblyPatcher.Patch(assemblyInfoFiles.ToReadOnly(), assemblyVersion,
                    assemblyFileVersion);
            };

        It should_have_created_a_backup_file = () => File.Exists(patchResult.First().FileBackupPath).ShouldBeFalse();

        It should_have_created_a_backup_file_with_the_original_file_version =
            () => patchResult.First().OldAssemblyFileVersion.Version.ShouldEqual(new Version(1, 2, 3, 4));

        It should_have_created_a_backup_file_with_the_original_version =
            () => patchResult.First().OldAssemblyVersion.Version.ShouldEqual(new Version(1, 2, 0, 0));

        It should_have_have_the_same_line_count =
            () =>
            {
                string patchFullPath = patchResult.First().FullPath;
                int patchLineCount = File.ReadAllLines(patchFullPath).Count();
                string originalFullPath = assemblyInfoFiles.First().FullPath;
                int originalLineCount = File.ReadAllLines(originalFullPath).Count();
                patchLineCount.ShouldEqual(originalLineCount);
            };

        It should_have_modified_the_target_version =
            () => File.ReadAllText(patchResult.First().FullPath).ShouldContain("2.3.4.5");

        It should_have_patched_one_file = () => patchResult.Count.ShouldEqual(1);

        It should_still_have_the_target_assembly_file_info_exist =
            () => File.Exists(patchResult.First().FullPath).ShouldBeTrue();
    }
}