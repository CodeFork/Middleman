Param(
  [string]$distPath
)

if ($distPath -eq $null -or $distPath -eq '')
{
    $distPath = [System.IO.Directory]::GetCurrentDirectory();
}

[System.IO.Directory]::SetCurrentDirectory($distPath);

if ([System.IO.File]::Exists("./Middleman.zip"))
{
    [System.IO.File]::Delete("./Middleman.zip");
}

if ([System.IO.Directory]::Exists("./dist/"))
{
    Add-Type -Assembly "System.IO.Compression.FileSystem" ;
    [System.IO.Compression.ZipFile]::CreateFromDirectory("./dist/", "./Middleman.zip");
}
