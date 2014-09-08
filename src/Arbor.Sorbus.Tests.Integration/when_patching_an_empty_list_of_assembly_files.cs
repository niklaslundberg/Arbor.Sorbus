using System;
using System.Collections.Generic;
using Arbor.Aesculus.Core;
using Arbor.Sorbus.Core;
using Machine.Specifications;

namespace Arbor.Sorbus.Tests.Integration
{
    [Subject(typeof (AssemblyPatcher))]
    public class when_patching_an_empty_list_of_assembly_files : patch_assembly_info_base
    {
        static AssemblyPatcher assemblyPatcher;
        static IEnumerable<AssemblyInfoFile> assemblyInfoFiles;
        static AssemblyVersion assemblyVersion;
        static AssemblyFileVersion assemblyFileVersion;
        static PatchResult patchResult;

        Establish context = () =>
            {
                assemblyPatcher = new AssemblyPatcher();
                assemblyInfoFiles = new List<AssemblyInfoFile>();
                assemblyVersion = new AssemblyVersion(new Version(1, 0, 0, 0));
                assemblyFileVersion = new AssemblyFileVersion(new Version(1, 0, 0, 0));
            };

        Because of =
            () =>
                {
                    patchResult = assemblyPatcher.Patch(assemblyInfoFiles.ToReadOnly(), assemblyVersion,
                                                        assemblyFileVersion, VcsPathHelper.FindVcsRootPath());
                };

        It should_have_patched_no_files = () => patchResult.Count.ShouldEqual(0);
    }
}