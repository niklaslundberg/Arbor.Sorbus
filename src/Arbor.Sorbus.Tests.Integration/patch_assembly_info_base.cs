using System;
using System.IO;
using Arbor.Aesculus.Core;
using Machine.Specifications;

namespace Arbor.Sorbus.Tests.Integration
{
    public abstract class patch_assembly_info_base
    {
        Establish context = () =>
            {
                var startDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var assemblyInfoPath = Path.Combine(
                    VcsPathHelper.FindVcsRootPath(startDirectory), "src",
                    "Arbor.Sorbus.Tests.Integration", "AssemblyInfo.cs");

                var originalfile = new FileInfo(assemblyInfoPath);

                var destinationFile = new FileInfo(Path.Combine(startDirectory, "AssemblyInfo.cs"));

                if (destinationFile.Exists)
                {
                    destinationFile.Delete();
                }

                originalfile.CopyTo(destinationFile.FullName);
            };
    }
}