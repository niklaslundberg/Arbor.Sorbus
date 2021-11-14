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
    public class when_patching_assembly_info_with_metadata
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
            string startDirectory = AppDomain.CurrentDomain.BaseDirectory;
            fileName = "AssemblyInfoMetadata.cs";
            string assemblyInfoPath2 = Path.Combine(
               VcsTestPathHelper.TryFindVcsRootPath()!, "tests",
                "Arbor.Sorbus.Tests.Integration", fileName);

            var originalfile = new FileInfo(assemblyInfoPath2);

            var destinationFile = new FileInfo(Path.Combine(startDirectory, fileName));

            if (destinationFile.Exists)
            {
                destinationFile.Delete();
            }

            originalfile.CopyTo(destinationFile.FullName);

            assemblyPatcher = new AssemblyPatcher(VcsTestPathHelper.TryFindVcsRootPath()!,
                Console.WriteLine);
            assemblyInfoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);


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
                assemblyFileVersion, assemblyMetaData: new AssemblyMetaData("ModifiedDescription", "ModifiedConfiguration","ModifiedCompany", "ModifiedProduct", "ModifiedCopyright", "ModifiedTrademark"));
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

        It should_have_original_company_metadata_set_to_old_value = () =>
        {
            unpatchedResults.First().First().NewAssemblyMetadata.Company.ShouldEqual("CustomCompany");
        };
        It should_have_original_configuration_metadata_set_to_old_value = () =>
        {
            unpatchedResults.First().First().NewAssemblyMetadata.Configuration.ShouldEqual("CustomConfiguration");
        };
        It should_have_original_product_metadata_set_to_old_value = () =>
        {
            unpatchedResults.First().First().NewAssemblyMetadata.Product.ShouldEqual("CustomProduct");
        };
        It should_have_original_trademark_metadata_set_to_old_value = () =>
        {
            unpatchedResults.First().First().NewAssemblyMetadata.Trademark.ShouldEqual("CustomTrademark");
        };
        It should_have_original_copyright_metadata_set_to_old_value = () =>
        {
            unpatchedResults.First().First().NewAssemblyMetadata.Copyright.ShouldEqual("CustomCopyright");
        };
        It should_have_original_description_metadata_set_to_old_value = () =>
        {
            unpatchedResults.First().First().NewAssemblyMetadata.Description.ShouldEqual("CustomDescription");
        };

        It should_have_company_metadata_set_to_new_value = () =>
        {
            patchResult.First().NewAssemblyMetadata.Company.ShouldEqual("ModifiedCompany");
        };

        It should_have_configuration_metadata_set_to_new_value = () =>
        {
            patchResult.First().NewAssemblyMetadata.Configuration.ShouldEqual("ModifiedConfiguration");
        };
        It should_have_product_metadata_set_to_new_value = () =>
        {
            patchResult.First().NewAssemblyMetadata.Product.ShouldEqual("ModifiedProduct");
        };
        It should_have_trademark_metadata_set_to_new_value = () =>
        {
            patchResult.First().NewAssemblyMetadata.Trademark.ShouldEqual("ModifiedTrademark");
        };
        It should_have_copyright_metadata_set_to_new_value = () =>
        {
            patchResult.First().NewAssemblyMetadata.Copyright.ShouldEqual("ModifiedCopyright");
        };
        It should_have_description_metadata_set_to_new_value = () =>
        {
            patchResult.First().NewAssemblyMetadata.Description.ShouldEqual("ModifiedDescription");
        };

        static string fileName;

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