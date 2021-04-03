using System.IO;
using Arbor.Aesculus.Core;

namespace Arbor.Sorbus.Tests.Integration
{
    public static class VcsTestPathHelper
    {
        public static string FindVcsRootPath(string baseDir = null)
        {
            if (NCrunch.Framework.NCrunchEnvironment.NCrunchIsResident())
            {
                DirectoryInfo originalDirectory = new FileInfo(NCrunch.Framework.NCrunchEnvironment.GetOriginalSolutionPath()).Directory;
                return VcsPathHelper.FindVcsRootPath(originalDirectory?.FullName);
            }

            return VcsPathHelper.FindVcsRootPath(baseDir);
        }
    }
}