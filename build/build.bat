SET Arbor.X.Build.Bootstrapper.AllowPrerelease=true
SET Arbor.X.Vcs.Branch.Name=develop
SET Arbor.X.Build.NetAssembly.PatchingEnabled=false
SET Arbor.X.Tools.External.MSpec.Enabled=

SET Version.Major=0
SET Version.Minor=1
SET Version.Patch=23
SET Version.Build=1

CALL "%~dp0\Build.exe"
