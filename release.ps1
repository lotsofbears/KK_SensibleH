if ($PSScriptRoot -match '.+?\\bin\\?') {
    $dir = $PSScriptRoot + "\"
}
else {
    $dir = $PSScriptRoot + "\bin\"
}

$out = $dir + "\out"
Remove-Item -Force -Path ($out) -Recurse -ErrorAction SilentlyContinue

# KK -------------------------------------
Write-Output ("Creating KK_SensibleH release")

New-Item -ItemType Directory -Force -Path ($out + "\BepInEx\plugins\KK_MainGameVR") | Out-Null

Copy-Item -Path ($dir + "\KK\*") -Destination ($out + "\BepInEx\plugins\KK_MainGameVR") -ErrorAction Stop -Force | Out-Null
# Copy-Item copies empty directories and I don't see any way to tell it to only copy files

$ver = "v" + (Get-ChildItem -Path ($dir + "\KK\KK_SensibleH.dll") -Force -ErrorAction Stop)[0].VersionInfo.FileVersion.ToString() -replace "([\d.]+?\d+)[\.0]*$", '${0}'
Write-Output ("Version " + $ver)
Compress-Archive -Path ($out + "\*") -Force -CompressionLevel "Optimal" -DestinationPath ($dir +"KK_SensibleH_" + $ver + ".zip")

Remove-Item -Force -Path ($out) -Recurse -ErrorAction SilentlyContinue

# KKS ------------------------------------
Write-Output ("Creating KKS_SensibleH release")

New-Item -ItemType Directory -Force -Path ($out + "\BepInEx\plugins\KKS_VR") | Out-Null

Copy-Item -Path ($dir + "\KKS\*") -Destination ($out + "\BepInEx\plugins\KKS_VR") -Force -ErrorAction Stop | Out-Null
# Copy-Item copies empty directories and I don't see any way to tell it to only copy files

$ver = "v" + (Get-ChildItem -Path ($dir + "\KKS\KKS_SensibleH.dll") -Force -ErrorAction Stop)[0].VersionInfo.FileVersion.ToString() -replace "([\d.]+?\d+)[\.0]*$", '${0}'
Write-Output ("Version " + $ver)
Compress-Archive -Path ($out + "\*") -Force -CompressionLevel "Optimal" -DestinationPath ($dir +"KKS_SensibleH_" + $ver + ".zip")

Remove-Item -Force -Path ($out) -Recurse -ErrorAction SilentlyContinue
