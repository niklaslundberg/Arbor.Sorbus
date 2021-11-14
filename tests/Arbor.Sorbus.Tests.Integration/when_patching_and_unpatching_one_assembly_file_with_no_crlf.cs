using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Arbor.Aesculus.Core;
using Arbor.Aesculus.NCrunch;
using Arbor.Sorbus.Core;
using Machine.Specifications;

namespace Arbor.Sorbus.Tests.Integration
{
    [Subject(typeof (AssemblyPatcher))]
    public class when_patching_and_unpatching_one_assembly_file_with_no_crlf : patch_assembly_info_base_no_crlf
    {
        static AssemblyPatcher assemblyPatcher;
        static IEnumerable<AssemblyInfoFile> assemblyInfoFiles;
        static AssemblyVersion assemblyVersion;
        static AssemblyFileVersion assemblyFileVersion;
        static PatchResult patchResult;
        static IEnumerable<PatchResult> unpatchedResults;
        static byte[] originalHash;
        static string assemblyInfoPath;
        static byte[] unpatchedHash;
        static string originalText;
        static string unpatchedText;

        Establish context = () =>
        {
            assemblyPatcher = new AssemblyPatcher(VcsTestPathHelper.TryFindVcsRootPath()!,
                Console.WriteLine);
            assemblyInfoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AssemblyInfoMissingCRLF.cs");


            assemblyInfoFiles = new List<AssemblyInfoFile>
                                {
                                    new AssemblyInfoFile(assemblyInfoPath)
                                };
            assemblyVersion = new AssemblyVersion(new Version(2, 3, 0, 0));
            assemblyFileVersion = new AssemblyFileVersion(new Version(2, 3, 4, 5));

            patchResult = assemblyPatcher.Patch(assemblyInfoFiles.ToReadOnly(),
                new AssemblyVersion(new Version(1, 2, 0, 0)),
                new AssemblyFileVersion(new Version(1, 2, 3, 4)));

            originalHash = ComputeHash(assemblyInfoPath);
            originalText = File.ReadAllText(assemblyInfoPath, Encoding.UTF8);

            patchResult = assemblyPatcher.Patch(assemblyInfoFiles.ToReadOnly(), assemblyVersion,
                assemblyFileVersion);
        };

        Because of =
            () =>
            {
                unpatchedResults = assemblyPatcher.Unpatch(patchResult);
                unpatchedHash = ComputeHash(assemblyInfoPath);

                unpatchedText = File.ReadAllText(assemblyInfoPath, Encoding.UTF8);
            };

        It should_have_created_a_backup_file = () => File.Exists(patchResult.First().FileBackupPath).ShouldBeFalse();

        It should_have_created_a_backup_file_with_the_original_file_version =
            () => patchResult.First().OldAssemblyFileVersion.Version.ShouldEqual(new Version(1, 2, 3, 4));

        It should_have_created_a_backup_file_with_the_original_version =
            () => patchResult.First().OldAssemblyVersion.Version.ShouldEqual(new Version(1, 2, 0, 0));

        It should_have_have_the_same_hash_as_the_original =
            () => BitConverter.ToString(originalHash).ShouldEqual(BitConverter.ToString(unpatchedHash));

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
            () => File.ReadAllText(patchResult.First().FullPath).ShouldContain("1.2.3.4");

        It should_have_patched_one_file = () => patchResult.Count.ShouldEqual(1);
        It should_have_the_same_character_count = () => originalText.Length.ShouldEqual(unpatchedText.Length);

        It should_have_the_same_text = () => originalText.ShouldEqual(unpatchedText);

        It should_still_have_the_target_assembly_file_info_exist =
            () => File.Exists(patchResult.First().FullPath).ShouldBeTrue();

        static byte[] ComputeHash(string file)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(file))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }
    }
}