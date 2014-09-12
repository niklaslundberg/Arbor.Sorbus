using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Arbor.Sorbus.Core
{
    public sealed class AssemblyPatcher
    {
        public const string Patchedassemblyinfos = "_patchedAssemblyInfos";
        readonly string _sourceBase;

        public AssemblyPatcher(string sourceBase)
        {
            if (string.IsNullOrWhiteSpace(sourceBase))
            {
                throw new ArgumentNullException("sourceBase");
            }

            if (!Directory.Exists(sourceBase))
            {
                throw new IOException(string.Format("The specified source base directory '{0}' does not exist", sourceBase));
            }

            _sourceBase = sourceBase;
        }

        public string BackupBasePath()
        {
            return Path.Combine(_sourceBase, Patchedassemblyinfos);
        }

        public IEnumerable<PatchResult> Unpatch(PatchResult patchResult)
        {
            List<PatchResult> result =
                patchResult.Select(
                    file =>
                    {
                        var assemblyInfoFile = new AssemblyInfoFile(file.FullPath);
                        return Patch(assemblyInfoFile, file.OldAssemblyVersion, file.OldAssemblyFileVersion);
                    }).ToList();

            DeleteEmptySubDirs(new DirectoryInfo(BackupBasePath()), true);

            return result;
        }

        PatchResult Patch(AssemblyInfoFile assemblyInfoFile,
            AssemblyVersion assemblyVersion,
            AssemblyFileVersion assemblyFileVersion)
        {
            CheckArguments(assemblyInfoFile, assemblyVersion, assemblyFileVersion);


            var patchResult = new PatchResult();

            AssemblyInfoPatchResult result = PatchAssemblyInfo(assemblyVersion, assemblyFileVersion, assemblyInfoFile);

            patchResult.Add(result);

            return patchResult;
        }

        public PatchResult Patch(IReadOnlyCollection<AssemblyInfoFile> assemblyInfoFiles,
            AssemblyVersion assemblyVersion,
            AssemblyFileVersion assemblyFileVersion, string sourceBase)
        {
            CheckArguments(assemblyInfoFiles, assemblyVersion, assemblyFileVersion);

            if (!assemblyInfoFiles.Any())
            {
                return new PatchResult();
            }

            var patchResult = new PatchResult();


            List<AssemblyInfoPatchResult> patchResults = assemblyInfoFiles
                .Select(assemblyInfoFile =>
                    PatchAssemblyInfo(assemblyVersion, assemblyFileVersion, assemblyInfoFile))
                .ToList();


            patchResults.ForEach(patchResult.Add);

            return patchResult;
        }

        public void SavePatchResult(PatchResult patchResult)
        {
            string resultFilePath = Path.Combine(BackupBasePath(), "Patched.txt");

            if (!File.Exists(resultFilePath))
            {
                using (File.Create(resultFilePath))
                {
                }
            }

            DeleteEmptySubDirs(new DirectoryInfo(BackupBasePath()));

            string json = JsonConvert.SerializeObject(patchResult.ToArray(), Formatting.Indented);

            File.WriteAllText(resultFilePath, json, Encoding.UTF8);
        }

        void DeleteEmptySubDirs(DirectoryInfo currentDir, bool deleteSelf = false)
        {
            currentDir.Refresh();
            if (!currentDir.Exists)
            {
                return;
            }

            foreach (DirectoryInfo subDir in currentDir.EnumerateDirectories())
            {
                DeleteEmptySubDirs(subDir);

                if (!subDir.EnumerateDirectories().Any() &&
                    !subDir.EnumerateFiles().Any())
                {
                    subDir.Delete();
                }
            }

            if (deleteSelf)
            {
                if (!currentDir.EnumerateDirectories().Any() &&
                    !currentDir.EnumerateFiles().Any())
                {
                    currentDir.Refresh();
                    if (currentDir.Exists)
                    {
                        currentDir.Delete();
                    }
                }
            }
        }

        AssemblyInfoPatchResult PatchAssemblyInfo(AssemblyVersion assemblyVersion,
            AssemblyFileVersion assemblyFileVersion,
            AssemblyInfoFile assemblyInfoFile)
        {
            string backupBasePath = BackupBasePath();

            var backupDirectory = new DirectoryInfo(backupBasePath);

            if (!backupDirectory.Exists)
            {
                backupDirectory.Create();
                backupDirectory.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }

            var sourceFileName = new FileInfo(assemblyInfoFile.FullPath);

            string relativePath = sourceFileName.FullName.Substring(_sourceBase.Length);

            string fileBackupPath = Path.Combine(BackupBasePath(), relativePath);

            var backupFile = new FileInfo(fileBackupPath);

            DirectoryInfo fileBackupBackupDirectory = backupFile.Directory;

            if (!fileBackupBackupDirectory.Exists)
            {
                fileBackupBackupDirectory.Create();
            }

            File.Copy(sourceFileName.FullName, fileBackupPath, true);


            AssemblyVersion oldAssemblyVersion = null;
            AssemblyFileVersion oldAssemblyFileVersion = null;
            Encoding encoding = Encoding.UTF8;

            string tmpPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cs");
            File.Copy(sourceFileName.FullName, tmpPath);
            using (var reader = new StreamReader(backupFile.FullName, encoding))
            {
                using (var writer = new StreamWriter(tmpPath, false, encoding))
                {
                    while (!reader.EndOfStream)
                    {
                        string readLine = reader.ReadLineWithEol();

                        string writtenLine;

                        bool isAssemblyVersionLine =
                            readLine.IndexOf("[assembly: AssemblyVersion(",
                                StringComparison.InvariantCultureIgnoreCase) >= 0;


                        bool isAssemblyFileVersionLine =
                            readLine.IndexOf("[assembly: AssemblyFileVersion(",
                                StringComparison.InvariantCultureIgnoreCase) >= 0;

                        bool lineIsComment = readLine.Trim().StartsWith("//");

                        if (lineIsComment)
                        {
                            writtenLine = readLine;
                        }
                        else if (isAssemblyVersionLine)
                        {
                            oldAssemblyVersion = new AssemblyVersion(ParseVersion(readLine));

                            writtenLine = string.Format("[assembly: AssemblyVersion(\"{0}.{1}.{2}.{3}\")]",
                                assemblyVersion.Version.Major,
                                assemblyVersion.Version.Minor,
                                assemblyVersion.Version.Build,
                                assemblyVersion.Version.Revision) + readLine.NewLine();
                        }
                        else if (isAssemblyFileVersionLine)
                        {
                            oldAssemblyFileVersion = new AssemblyFileVersion(ParseVersion(readLine));
                            writtenLine = string.Format("[assembly: AssemblyFileVersion(\"{0}.{1}.{2}.{3}\")]",
                                assemblyFileVersion.Version.Major,
                                assemblyFileVersion.Version.Minor,
                                assemblyFileVersion.Version.Build,
                                assemblyFileVersion.Version.Revision) + readLine.NewLine();
                        }
                        else
                        {
                            writtenLine = readLine;
                        }

                        writer.Write(writtenLine);
                    }
                }
            }

            if (oldAssemblyVersion == null)
            {
                throw new InvalidOperationException("Could not find assembly version in file " +
                                                    assemblyInfoFile.FullPath);
            }
            if (oldAssemblyFileVersion == null)
            {
                throw new InvalidOperationException("Could not find assembly file version in file " +
                                                    assemblyInfoFile.FullPath);
            }


            if (assemblyVersion.Version != oldAssemblyVersion.Version ||
                assemblyFileVersion.Version != oldAssemblyFileVersion.Version)
            {
                if (File.Exists(sourceFileName.FullName))
                {
                    File.Delete(sourceFileName.FullName);
                }
                File.Copy(tmpPath, sourceFileName.FullName);
            }
            File.Delete(tmpPath);
            var result = new AssemblyInfoPatchResult(assemblyInfoFile.FullPath, fileBackupPath,
                oldAssemblyVersion, assemblyVersion, oldAssemblyFileVersion,
                assemblyFileVersion);

            File.Delete(backupFile.FullName);

            if (!fileBackupBackupDirectory.EnumerateDirectories().Any() &&
                !fileBackupBackupDirectory.EnumerateFiles().Any())
            {
                fileBackupBackupDirectory.Delete();
            }

            return result;
        }

        Version ParseVersion(string readLine)
        {
            if (string.IsNullOrWhiteSpace(readLine))
            {
                throw new ArgumentNullException("readLine");
            }

            int openQuotePosition = readLine.IndexOf('"');

            string leftTrimmed = readLine.Substring(openQuotePosition + 1);

            int closeQuotePosition = leftTrimmed.IndexOf('"');

            string rigthTrimmed = leftTrimmed.Substring(0, closeQuotePosition);

            string starReplaced = rigthTrimmed.Replace('*', '0');

            return Version.Parse(starReplaced);
        }

        static void CheckArguments(AssemblyInfoFile assemblyInfoFile, AssemblyVersion assemblyVersion,
            AssemblyFileVersion assemblyFileVersion)
        {
            if (assemblyInfoFile == null)
            {
                throw new ArgumentNullException("assemblyInfoFile");
            }

            if (assemblyVersion == null)
            {
                throw new ArgumentNullException("assemblyVersion");
            }
            if (assemblyFileVersion == null)
            {
                throw new ArgumentNullException("assemblyFileVersion");
            }
        }

        static void CheckArguments(IEnumerable<AssemblyInfoFile> assemblyInfoFiles, AssemblyVersion assemblyVersion,
            AssemblyFileVersion assemblyFileVersion)
        {
            if (assemblyInfoFiles == null)
            {
                throw new ArgumentNullException("assemblyInfoFiles");
            }

            if (assemblyVersion == null)
            {
                throw new ArgumentNullException("assemblyVersion");
            }
            if (assemblyFileVersion == null)
            {
                throw new ArgumentNullException("assemblyFileVersion");
            }
        }
    }
}