using System;
using System.Collections.Generic;
using Arbor.Aesculus.Core;
using Arbor.Sorbus.Core;
using Machine.Specifications;

namespace Arbor.Sorbus.Tests.Integration
{
    [Subject(typeof (AssemblyPatcher))]
    public class when_patching_with_a_null_assembly_file_version : patch_assembly_info_base
    {
        static AssemblyPatcher assemblyPatcher;
        static IEnumerable<AssemblyInfoFile> assemblyInfoFiles;
        static AssemblyVersion assemblyVersion;
        static AssemblyFileVersion assemblyFileVersion;
        static PatchResult patchResult;
        static Exception exception;

        Establish context = () =>
        {
            assemblyPatcher = new AssemblyPatcher(VcsPathHelper.FindVcsRootPath(),
                new ConsoleLogger() {LogLevel = LogLevel.Debug});
            assemblyInfoFiles = new List<AssemblyInfoFile>();
            assemblyVersion = new AssemblyVersion(new Version(1, 0, 0, 0));
            assemblyFileVersion = null;
        };

        Because of =
            () =>
            {
                exception =
                    Catch.Exception(
                        () =>
                            patchResult =
                                assemblyPatcher.Patch(assemblyInfoFiles.ToReadOnly(), assemblyVersion,
                                    assemblyFileVersion));
            };

        It should_throw_argument_null_exception = () => exception.ShouldBeOfExactType<ArgumentNullException>();
        It should_throw_exception = () => exception.ShouldNotBeNull();

        It should_throw_have_argument_name_assembly_file_version =
            () => ((ArgumentNullException) exception).ParamName.ShouldEqual("assemblyFileVersion");
    }
}