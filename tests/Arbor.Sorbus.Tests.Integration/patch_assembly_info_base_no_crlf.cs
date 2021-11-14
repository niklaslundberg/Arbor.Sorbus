using System;
using System.IO;
using Arbor.Aesculus.Core;
using Arbor.Aesculus.NCrunch;
using Machine.Specifications;

namespace Arbor.Sorbus.Tests.Integration
{
    public abstract class patch_assembly_info_base_no_crlf
    {
        Establish context = () =>
        {
            string startDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string assemblyInfoPath = Path.Combine(VcsTestPathHelper.TryFindVcsRootPath()!, "tests",
                "Arbor.Sorbus.Tests.Integration", "AssemblyInfoMissingCRLF.cs");

            var originalfile = new FileInfo(assemblyInfoPath);

            var destinationFile = new FileInfo(Path.Combine(startDirectory, "AssemblyInfoMissingCRLF.cs"));

            if (destinationFile.Exists)
            {
                destinationFile.Delete();
            }

            originalfile.CopyTo(destinationFile.FullName);
        };
    }
}