using System;
using System.IO;
using Arbor.Aesculus.Core;
using Arbor.Sorbus.Core;
using Machine.Specifications;

namespace Arbor.Sorbus.Tests.Integration
{
    [Subject(typeof (AssemblyPatcherApp))]
    public class when_patching_other_file_name_pattern
    {
        static AssemblyPatcherApp app;
        static string sourceBase;
        Cleanup after = () => app.Unpatch(sourceBase);

        Establish context = () => { app = new AssemblyPatcherApp(Console.WriteLine);};

        Because of = () =>
        {
            sourceBase = Path.Combine(VcsTestPathHelper.FindVcsRootPath(), "src",
                "Arbor.Sorbus.Tests.Integration", "OtherNames", "Multiple");
            app.Patch(new AssemblyVersion(new Version("1.2.3.4")), new AssemblyFileVersion(new Version("1.2.0.0")),
                sourceBase, "*_TestPatch.cs");
        };

        It should_succeed = () => { };
    }
}