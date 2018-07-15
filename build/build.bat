SET Arbor.X.Build.NetAssembly.PatchingEnabled=false
SET Arbor.X.Tools.External.MSpec.Enabled=
SET Arbor.X.MSBuild.NuGetRestore.Enabled=true

CALL "%~dp0\Build.exe"

EXIT /B %ERRORLEVEL%