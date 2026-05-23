# APK/XAPK Unpack - Patch - Repack Workflow

This workflow covers patching a Unity IL2CPP Android game packaged as APK or XAPK.

## Required Tools

- Java JDK
- Android SDK Build Tools
  - `apksigner`
  - `zipalign`
  - `aapt2`
- Android Platform Tools
  - `adb`
- 7-Zip or WinRAR
- apktool
- Il2CppDumper
- Optional: IDA, Ghidra, HxD, ImHex

Example SDK paths:

```text
E:\AndroidSDK\build-tools\30.0.3\apksigner.bat
E:\AndroidSDK\build-tools\30.0.3\zipalign.exe
E:\AndroidSDK\platform-tools\adb.exe
```

## 1. Unpack XAPK

Rename `.xapk` to `.zip`, then extract it.

Typical output:

```text
base.apk
config.arm64_v8a.apk
manifest.json
icon.png
```

For Unity IL2CPP, `libil2cpp.so` is usually in:

```text
config.arm64_v8a.apk\lib\arm64-v8a\libil2cpp.so
```

## 2. Dump IL2CPP

Use:

```powershell
cd E:\Tools\Il2CppDumper-win-v6.7.46
.\Il2CppDumper.exe .\libil2cpp.so .\global-metadata.dat .
```

Useful outputs:

```text
dump.cs
script.json
il2cpp.h
DummyDll\
```

Use `Offset`, not `RVA`, when patching bytes directly in the `.so`.

## 3. Patch libil2cpp.so

For ARM64, returning integer `10000` is:

```asm
mov w0, #10000
ret
```

Bytes:

```text
00 E2 84 52 C0 03 5F D6
```

Example PowerShell patch:

```powershell
$path = "E:\Tools\Il2CppDumper-win-v6.7.46\libil2cpp.so"
Copy-Item $path "$path.bak"

$bytes = [System.IO.File]::ReadAllBytes($path)
$patchReturn10000 = [byte[]](0x00,0xE2,0x84,0x52,0xC0,0x03,0x5F,0xD6)

function PatchBytes($offset, [byte[]]$patch) {
    for ($i = 0; $i -lt $patch.Length; $i++) {
        $bytes[$offset + $i] = $patch[$i]
    }
}

PatchBytes 0x309B590 $patchReturn10000
PatchBytes 0x309B8F0 $patchReturn10000
PatchBytes 0x309B984 $patchReturn10000

[System.IO.File]::WriteAllBytes($path, $bytes)
```

For a `void` setter, use ARM64 `ret`:

```text
C0 03 5F D6
```

## 4. Replace libil2cpp.so in Split APK

Do not use `apktool b` for native-only split APKs unless needed. It can fail on split manifest attributes.

Instead, replace the entry in the ZIP/APK:

```powershell
$apk = "F:\Downloads\Bubble Pop\Bubble Pop\config.arm64_v8a.apk"
$patchedSo = "E:\Tools\Il2CppDumper-win-v6.7.46\libil2cpp.so"
$out = "F:\Downloads\Bubble Pop\patched\config.arm64_v8a.unsigned.apk"

Copy-Item $apk $out -Force

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$zip = [System.IO.Compression.ZipFile]::Open($out, [System.IO.Compression.ZipArchiveMode]::Update)
try {
    $entry = $zip.GetEntry("lib/arm64-v8a/libil2cpp.so")
    if ($entry -ne $null) { $entry.Delete() }

    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
        $zip,
        $patchedSo,
        "lib/arm64-v8a/libil2cpp.so",
        [System.IO.Compression.CompressionLevel]::NoCompression
    ) | Out-Null
}
finally {
    $zip.Dispose()
}
```

## 5. Strip Old Signatures

Remove old signature files from every APK you will sign:

```powershell
function Strip-Signature([string]$apk) {
    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem

    $zip = [System.IO.Compression.ZipFile]::Open($apk, [System.IO.Compression.ZipArchiveMode]::Update)
    try {
        $entries = @($zip.Entries | Where-Object {
            $_.FullName -like "META-INF/*" -and
            ($_.FullName -like "*.RSA" -or
             $_.FullName -like "*.DSA" -or
             $_.FullName -like "*.EC" -or
             $_.FullName -like "*.SF" -or
             $_.FullName -like "*MANIFEST.MF")
        })

        foreach ($e in $entries) {
            $e.Delete()
        }
    }
    finally {
        $zip.Dispose()
    }
}
```

## 6. Zipalign Before Signing

Use `zipalign` before `apksigner`.

```powershell
$zipalign = "E:\AndroidSDK\build-tools\30.0.3\zipalign.exe"

& $zipalign -f 4 `
  "F:\Downloads\Bubble Pop\patched\com.bitmango.go.bubblepop.unsigned.apk" `
  "F:\Downloads\Bubble Pop\patched\com.bitmango.go.bubblepop.aligned.apk"

& $zipalign -f 4 `
  "F:\Downloads\Bubble Pop\patched\config.arm64_v8a.unsigned.apk" `
  "F:\Downloads\Bubble Pop\patched\config.arm64_v8a.aligned.apk"
```

This avoids:

```text
Targeting R+ requires the resources.arsc of installed APKs to be stored uncompressed and aligned on a 4-byte boundary
```

## 7. Sign with apksigner

Do not use only `jarsigner` for Android 7+ / modern devices. It produces v1 signatures only.

Use `apksigner`:

```powershell
$apksigner = "E:\AndroidSDK\build-tools\30.0.3\apksigner.bat"
$ks = "$env:USERPROFILE\.android\debug.keystore"

& $apksigner sign `
  --ks $ks `
  --ks-key-alias androiddebugkey `
  --ks-pass pass:android `
  --key-pass pass:android `
  --v1-signing-enabled true `
  --v2-signing-enabled true `
  --v3-signing-enabled true `
  --out "F:\Downloads\Bubble Pop\patched\com.bitmango.go.bubblepop.signed.apk" `
  "F:\Downloads\Bubble Pop\patched\com.bitmango.go.bubblepop.aligned.apk"

& $apksigner sign `
  --ks $ks `
  --ks-key-alias androiddebugkey `
  --ks-pass pass:android `
  --key-pass pass:android `
  --v1-signing-enabled true `
  --v2-signing-enabled true `
  --v3-signing-enabled true `
  --out "F:\Downloads\Bubble Pop\patched\config.arm64_v8a.signed.apk" `
  "F:\Downloads\Bubble Pop\patched\config.arm64_v8a.aligned.apk"
```

Verify:

```powershell
& $apksigner verify --verbose --print-certs "F:\Downloads\Bubble Pop\patched\com.bitmango.go.bubblepop.signed.apk"
& $apksigner verify --verbose --print-certs "F:\Downloads\Bubble Pop\patched\config.arm64_v8a.signed.apk"
```

Expected:

```text
Verified using v1 scheme: true
Verified using v2 scheme: true
Verified using v3 scheme: true
```

## 8. Install Split APKs

Install base and split together:

```powershell
adb install-multiple --no-incremental `
  "F:\Downloads\Bubble Pop\patched\com.bitmango.go.bubblepop.signed.apk" `
  "F:\Downloads\Bubble Pop\patched\config.arm64_v8a.signed.apk"
```

If the original app is already installed with another certificate:

```powershell
adb uninstall com.bitmango.go.bubblepop
```

Then install again.

## Common Errors

### No signature found in package of version 2 or newer

Cause: APK was signed only with `jarsigner`.

Fix: sign with `apksigner` and enable v2/v3.

### Incremental installation not allowed

Cause: `adb` tried incremental install.

Fix:

```powershell
adb install-multiple --no-incremental base.apk split.apk
```

### resources.arsc must be uncompressed and 4-byte aligned

Cause: APK was signed/repacked without proper alignment.

Fix: run `zipalign` before `apksigner`.

### INSTALL_FAILED_UPDATE_INCOMPATIBLE

Cause: installed app was signed with a different certificate.

Fix:

```powershell
adb uninstall package.name
adb install-multiple --no-incremental base.apk split.apk
```

