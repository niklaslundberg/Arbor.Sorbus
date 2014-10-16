using System;
using System.Collections.Generic;
using Arbor.Aesculus.Core;
using Arbor.Sorbus.Core;
using Machine.Specifications;

namespace Arbor.Sorbus.Tests.Integration
{
    [Subject(typeof (AssemblyPatcher))]
    public class when_patching_a_null_list_of_assembly_files : patch_assembly_info_base
    {
        static AssemblyPatcher assemblyPatcher;
        static IReadOnlyCollection<AssemblyInfoFile> assemblyInfoFiles;
        static AssemblyVersion assemblyVersion = null;
        static AssemblyFileVersion assemblyFileVersion = null;
        static PatchResult patchResult;
        static Exception exception;

        Establish context = () =>
        {
            assemblyPatcher = new AssemblyPatcher(VcsPathHelper.FindVcsRootPath(),
                new ConsoleLogger() {LogLevel = LogLevel.Debug});
            assemblyInfoFiles = null;
        };

        Because of =
            () =>
            {
                exception =
                    Catch.Exception(
                        () =>
                            patchResult = assemblyPatcher.Patch(assemblyInfoFiles, assemblyVersion, assemblyFileVersion));
            };

        It should_throw_an_exception = () => exception.ShouldNotBeNull();
        It should_throw_argument_null_exception = () => exception.ShouldBeOfExactType<ArgumentNullException>();

        It should_throw_have_argument_name_assembly_info_files =
            () => ((ArgumentNullException) exception).ParamName.ShouldEqual("assemblyInfoFiles");
    }
}