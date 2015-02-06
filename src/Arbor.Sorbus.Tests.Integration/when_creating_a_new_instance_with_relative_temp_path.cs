using System.IO;
using Arbor.Sorbus.Core;
using Machine.Specifications;

namespace Arbor.Sorbus.Tests.Integration
{
    [Subject(typeof (AssemblyPatcher))]
    public class when_creating_a_new_instance_with_relative_temp_path
    {
        Establish context = () =>
        {
            sourceBase = Path.GetTempPath();
            tempPath = "customrelativetemp";

        };

        Because of = () =>
        {
            patcher = new AssemblyPatcher(sourceBase, tempPath: tempPath);
        };

        It should_have_folder_set_to_relative = () => patcher.BackupBasePath().ShouldEqual(Path.Combine(sourceBase, tempPath, AssemblyPatcher.Patchedassemblyinfos));
        static AssemblyPatcher patcher;
        static string sourceBase;
        static string tempPath;
    }
}