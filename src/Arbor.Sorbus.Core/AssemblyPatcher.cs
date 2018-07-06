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
        public readonly string PatchedassemblyinfosPath;
        readonly string _sourceBase;
        readonly Action<string> _logger;

        public AssemblyPatcher(string sourceBase, Action<string> logger = null, string tempPath = null)
        {
            _logger = logger;

            if (string.IsNullOrWhiteSpace(sourceBase))
            {
                throw new ArgumentNullException(nameof(sourceBase));
            }

            if (!Directory.Exists(sourceBase))
            {
                throw new IOException($"The specified source base directory '{sourceBase}' does not exist");
            }

            if (!string.IsNullOrWhiteSpace(tempPath))
            {
                if (Path.IsPathRooted(tempPath))
                {
                    PatchedassemblyinfosPath = Path.Combine(tempPath, Patchedassemblyinfos);
                }
                else
                {
                    PatchedassemblyinfosPath = Path.Combine(sourceBase, tempPath, Patchedassemblyinfos);
                }
            }
            else
            {
                PatchedassemblyinfosPath = Path.Combine(sourceBase, "build", "temp", Patchedassemblyinfos);
            }

            _sourceBase = sourceBase;
        }

        public string BackupBasePath()
        {
            return Path.Combine(_sourceBase, PatchedassemblyinfosPath);
        }

        public IEnumerable<PatchResult> Unpatch(PatchResult patchResult)
        {
            _logger?.Invoke(
                $"Unpatching from patch result {string.Join(Environment.NewLine, patchResult.Select(item => item.ToString()))}");

            List<PatchResult> result =
                patchResult.Select(
                    file =>
                    {
                        var assemblyInfoFile = new AssemblyInfoFile(file.FullPath);
                        return Patch(assemblyInfoFile, file.OldAssemblyVersion, file.OldAssemblyFileVersion, file.OldAssemblyMetadata);
                    }).ToList();

            DeleteEmptySubDirs(new DirectoryInfo(BackupBasePath()), true);

            return result;
        }

        PatchResult Patch(AssemblyInfoFile assemblyInfoFile,
            AssemblyVersion assemblyVersion,
            AssemblyFileVersion assemblyFileVersion, AssemblyMetaData assemblyMetaData = null)
        {
            CheckArguments(assemblyInfoFile, assemblyVersion, assemblyFileVersion);


            var patchResult = new PatchResult();

            AssemblyInfoPatchResult result = PatchAssemblyInfo(assemblyVersion, assemblyFileVersion, assemblyInfoFile, assemblyMetaData);

            patchResult.Add(result);

            return patchResult;
        }

        public PatchResult Patch(IReadOnlyCollection<AssemblyInfoFile> assemblyInfoFiles, AssemblyVersion assemblyVersion, AssemblyFileVersion assemblyFileVersion, AssemblyMetaData assemblyMetaData = null)
        {
            CheckArguments(assemblyInfoFiles, assemblyVersion, assemblyFileVersion);

            if (!assemblyInfoFiles.Any())
            {
                return new PatchResult();
            }

            var patchResult = new PatchResult();


            List<AssemblyInfoPatchResult> patchResults = assemblyInfoFiles
                .Select(assemblyInfoFile =>
                    PatchAssemblyInfo(assemblyVersion, assemblyFileVersion, assemblyInfoFile, assemblyMetaData))
                .ToList();

            patchResults.Where(result => result.Succeeded).ToList().ForEach(patchResult.Add);

            return patchResult;
        }

        public void SavePatchResult(PatchResult patchResult)
        {
            string backupBasePath = BackupBasePath();

            if (!Directory.Exists(backupBasePath))
            {
                Directory.CreateDirectory(backupBasePath);
            }

            string resultFilePath = Path.Combine(backupBasePath, "Patched.txt");

            if (!File.Exists(resultFilePath))
            {
                using (File.Create(resultFilePath))
                {
                }
            }

            DeleteEmptySubDirs(new DirectoryInfo(backupBasePath));

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
                        _logger?.Invoke($"Deleting directory '{currentDir.FullName}'");
                        currentDir.Delete();
                    }
                }
            }
        }

        AssemblyInfoPatchResult PatchAssemblyInfo(AssemblyVersion assemblyVersion,
            AssemblyFileVersion assemblyFileVersion,
            AssemblyInfoFile assemblyInfoFile,
            AssemblyMetaData assemblyMetaData)
        {
            string backupBasePath = BackupBasePath();
            var backupDirectory = new DirectoryInfo(backupBasePath);

            _logger?.Invoke(
                $"Patching assembly file '{assemblyInfoFile.FullPath}', assembly version {assemblyVersion.Version}, assembly file version {assemblyFileVersion.Version}");

            try
            {
                _logger?.Invoke($"Assembly info patch backup directory is '{backupDirectory.FullName}'");
                if (!backupDirectory.Exists)
                {
                    _logger?.Invoke($"Creating assembly info patch backup directory '{backupDirectory.FullName}'");
                    backupDirectory.Create();
                    backupDirectory.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Backup base path is '{backupBasePath}', exists: {backupDirectory.Exists}", ex);
            }

            var sourceFileName = new FileInfo(assemblyInfoFile.FullPath);

            string relativePath = sourceFileName.FullName.Substring(_sourceBase.Length);

            string fileBackupPath = Path.Combine(BackupBasePath(), relativePath.TrimStart(Path.DirectorySeparatorChar));

            var backupFile = new FileInfo(fileBackupPath);

            DirectoryInfo fileBackupBackupDirectory = backupFile.Directory;

            if (fileBackupBackupDirectory == null)
            {
                throw new InvalidOperationException("The backup file directory is null");
            }

            if (!fileBackupBackupDirectory.Exists)
            {
                try
                {
                    _logger?.Invoke(
                        $"Creating assembly info patch backup directory '{fileBackupBackupDirectory.FullName}' for file '{assemblyInfoFile.FullPath}'");
                    fileBackupBackupDirectory.Create();
                }
                catch (Exception ex)
                {
                    throw new IOException(
                        $"Could not create directory '{fileBackupBackupDirectory.FullName}'", ex);
                }
            }

            _logger?.Invoke($"Copying file '{sourceFileName.FullName}' to backup '{fileBackupPath}'");
            File.Copy(sourceFileName.FullName, fileBackupPath, true);

            AssemblyVersion oldAssemblyVersion = null;
            AssemblyFileVersion oldAssemblyFileVersion = null;
            Encoding encoding = Encoding.UTF8;

            string tmpPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cs");

            _logger?.Invoke($"Copying file '{sourceFileName.FullName}' to temp file '{tmpPath}'");

            string oldDescription = null;
            string oldCompany = null;
            string oldCopyright = null;
            string oldTrademark = null;
            string oldProduct = null;
            string oldConfiguration = null;

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

                        bool isAssemblyDescriptionLine =
                            readLine.IndexOf("[assembly: AssemblyDescription(",
                                StringComparison.InvariantCultureIgnoreCase) >= 0;

                        bool isAssemblyCompanyLine =
                            readLine.IndexOf("[assembly: AssemblyCompany(",
                                StringComparison.InvariantCultureIgnoreCase) >= 0;

                        bool isAssemblyTrademarkLine =
                            readLine.IndexOf("[assembly: AssemblyTrademark(",
                                StringComparison.InvariantCultureIgnoreCase) >= 0;

                        bool isAssemblyCopyrightLine =
                            readLine.IndexOf("[assembly: AssemblyCopyright(",
                                StringComparison.InvariantCultureIgnoreCase) >= 0;

                        bool isAssemblyProductLine =
                            readLine.IndexOf("[assembly: AssemblyProduct(",
                                StringComparison.InvariantCultureIgnoreCase) >= 0;

                        bool isAssemblyConfigurationLine =
                            readLine.IndexOf("[assembly: AssemblyConfiguration(",
                                StringComparison.InvariantCultureIgnoreCase) >= 0;

                        bool lineIsComment = readLine.Trim().StartsWith("//");

                        if (lineIsComment)
                        {
                            writtenLine = readLine;
                        }
                        else if (isAssemblyVersionLine)
                        {
                            oldAssemblyVersion = new AssemblyVersion(ParseVersion(readLine));

                            writtenLine =
                                $"[assembly: AssemblyVersion(\"{assemblyVersion.Version.Major}.{assemblyVersion.Version.Minor}.{assemblyVersion.Version.Build}.{assemblyVersion.Version.Revision}\")]" + readLine.NewLine();
                        }
                        else if (isAssemblyFileVersionLine)
                        {
                            oldAssemblyFileVersion = new AssemblyFileVersion(ParseVersion(readLine));
                            writtenLine =
                                $"[assembly: AssemblyFileVersion(\"{assemblyFileVersion.Version.Major}.{assemblyFileVersion.Version.Minor}.{assemblyFileVersion.Version.Build}.{assemblyFileVersion.Version.Revision}\")]" + readLine.NewLine();
                        }
                        else if (assemblyMetaData != null)
                        {
                            if (isAssemblyDescriptionLine && assemblyMetaData.Description != null)
                            {
                                oldDescription = ParseAttribute(readLine);
                                writtenLine =
                                    $"[assembly: AssemblyDescription(\"{assemblyMetaData.Description}\")]{readLine.NewLine()}";
                            }
                            else if (isAssemblyConfigurationLine && assemblyMetaData.Configuration != null)
                            {
                                oldConfiguration = ParseAttribute(readLine);
                                writtenLine =
                                    $"[assembly: AssemblyConfiguration(\"{assemblyMetaData.Configuration}\")]{readLine.NewLine()}";
                            }
                            else if (isAssemblyCopyrightLine && assemblyMetaData.Copyright != null)
                            {
                                oldCopyright = ParseAttribute(readLine);
                                writtenLine =
                                    $"[assembly: AssemblyCopyright(\"{assemblyMetaData.Copyright}\")]{readLine.NewLine()}";
                            }
                            else if (isAssemblyCompanyLine && assemblyMetaData.Company != null)
                            {
                                oldCompany = ParseAttribute(readLine);
                                writtenLine =
                                    $"[assembly: AssemblyCompany(\"{assemblyMetaData.Company}\")]{readLine.NewLine()}";
                            }
                            else if (isAssemblyTrademarkLine && assemblyMetaData.Trademark != null)
                            {
                                oldTrademark = ParseAttribute(readLine);
                                writtenLine =
                                    $"[assembly: AssemblyTrademark(\"{assemblyMetaData.Trademark}\")]{readLine.NewLine()}";
                            }
                            else if (isAssemblyProductLine && assemblyMetaData.Product != null)
                            {
                                oldProduct = ParseAttribute(readLine);
                                writtenLine =
                                    $"[assembly: AssemblyProduct(\"{assemblyMetaData.Product}\")]{readLine.NewLine()}";
                            }
                            else
                            {
                                writtenLine = readLine;
                            }
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
                _logger?.Invoke($"Could not find assembly version in file {assemblyInfoFile.FullPath}");
            }
            if (oldAssemblyFileVersion == null)
            {
                _logger?.Invoke($"Could not find assembly file version in file {assemblyInfoFile.FullPath}");
            }
            AssemblyInfoPatchResult result;
            if (oldAssemblyVersion != null && oldAssemblyFileVersion != null)
            {

                if (assemblyVersion.Version != oldAssemblyVersion.Version ||
                    assemblyFileVersion.Version != oldAssemblyFileVersion.Version)
                {
                    if (File.Exists(sourceFileName.FullName))
                    {
                        File.Delete(sourceFileName.FullName);
                    }
                    File.Copy(tmpPath, sourceFileName.FullName);
                }

                var oldAssemblyMetadata = new AssemblyMetaData(oldDescription, oldConfiguration, oldCompany, oldProduct, oldCopyright, oldTrademark);

                result = new AssemblyInfoPatchResult(assemblyInfoFile.FullPath, fileBackupPath,
                    oldAssemblyVersion, assemblyVersion, oldAssemblyFileVersion,
                    assemblyFileVersion, newAssemblyMetadata: assemblyMetaData, oldAssemblyMetadata: oldAssemblyMetadata);
            }
            else
            {
                result = AssemblyInfoPatchResult.Failed(backupFile.FullName);
            }

            _logger?.Invoke($"Deleting temp file '{tmpPath}'");
            File.Delete(tmpPath);

            _logger?.Invoke($"Deleting backup file '{backupFile.FullName}'");
            File.Delete(backupFile.FullName);

            if (!fileBackupBackupDirectory.EnumerateDirectories().Any() &&
                !fileBackupBackupDirectory.EnumerateFiles().Any())
            {
                fileBackupBackupDirectory.Delete();
            }

            return result;
        }

        string ParseAttribute(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (value.StartsWith("[assembly: Assembly", StringComparison.InvariantCultureIgnoreCase))
            {
                var trimmed = value.Trim().Replace("[assembly: Assembly", "").Replace(@""")]", "");

                int startIndex = trimmed.IndexOf("(\"", StringComparison.InvariantCultureIgnoreCase);
                if (startIndex < 0)
                {
                    return value;
                }

                var attributeValue = trimmed.Substring(startIndex + 2);

                return attributeValue;
            }

            return value;
        }

        Version ParseVersion(string readLine)
        {
            if (string.IsNullOrWhiteSpace(readLine))
            {
                throw new ArgumentNullException(nameof(readLine));
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
                throw new ArgumentNullException(nameof(assemblyInfoFile));
            }

            if (assemblyVersion == null)
            {
                throw new ArgumentNullException(nameof(assemblyVersion));
            }
            if (assemblyFileVersion == null)
            {
                throw new ArgumentNullException(nameof(assemblyFileVersion));
            }
        }

        static void CheckArguments(IEnumerable<AssemblyInfoFile> assemblyInfoFiles, AssemblyVersion assemblyVersion,
            AssemblyFileVersion assemblyFileVersion)
        {
            if (assemblyInfoFiles == null)
            {
                throw new ArgumentNullException(nameof(assemblyInfoFiles));
            }

            if (assemblyVersion == null)
            {
                throw new ArgumentNullException(nameof(assemblyVersion));
            }
            if (assemblyFileVersion == null)
            {
                throw new ArgumentNullException(nameof(assemblyFileVersion));
            }
        }
    }
}