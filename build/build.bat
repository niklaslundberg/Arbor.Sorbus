@ECHO OFF
SET Arbor.X.Build.NetAssembly.PatchingEnabled=false
SET Arbor.X.Tools.External.MSpec.Enabled=
SET Arbor.X.MSBuild.NuGetRestore.Enabled=true
SET Arbor.X.Build.Bootstrapper.AllowPrerelease=true

CALL dotnet arbor-build

EXIT /B %ERRORLEVEL%