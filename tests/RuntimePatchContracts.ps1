param(
    [Parameter(Mandatory = $true)] [string] $ManagedGameFolder,
    [Parameter(Mandatory = $true)] [string] $BepInExCoreFolder,
    [string] $Plugin
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrEmpty($Plugin)) {
    $Plugin = Join-Path (Split-Path $PSScriptRoot -Parent) 'payload\BepInEx\plugins\RestoryItalianPatch\RestoryItalianPatch.dll'
}
foreach ($name in @('BepInEx.Core.dll', 'BepInEx.Unity.Mono.dll', '0Harmony.dll')) {
    [void][Reflection.Assembly]::LoadFrom((Join-Path $BepInExCoreFolder $name))
}
[void][Reflection.Assembly]::LoadFrom((Join-Path $ManagedGameFolder 'netstandard.dll'))
[void][Reflection.Assembly]::LoadFrom((Join-Path $ManagedGameFolder 'UnityEngine.dll'))
[void][Reflection.Assembly]::LoadFrom((Join-Path $ManagedGameFolder 'UnityEngine.CoreModule.dll'))
$gameAssembly = [Reflection.Assembly]::LoadFrom((Join-Path $ManagedGameFolder 'Restory.Assembly.dll'))
$pluginAssembly = [Reflection.Assembly]::LoadFrom($Plugin)

function Assert-True([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
}

$gameConfig = $gameAssembly.GetType('Restory.Data.GameConfigs.GameConfig', $true)
$languages = $gameConfig.GetProperty('SupportedLocalizations', [Reflection.BindingFlags]'Public,NonPublic,Instance,Static')
Assert-True ($null -ne $languages) 'GameConfig.SupportedLocalizations is missing.'
Assert-True ($languages.PropertyType.IsGenericType) 'SupportedLocalizations is not generic.'
Assert-True ($languages.PropertyType.GetGenericTypeDefinition().FullName -eq 'System.Collections.Generic.IReadOnlyCollection`1') 'SupportedLocalizations return type changed.'
Assert-True ($languages.PropertyType.GetGenericArguments()[0].FullName -eq 'UnityEngine.SystemLanguage') 'SupportedLocalizations element type changed.'

$settingsType = $gameAssembly.GetType('Restory.Gameplay.GameSettings.GameSettingsManager', $true)
$localization = $settingsType.GetProperty('Localization', [Reflection.BindingFlags]'Public,NonPublic,Instance,Static')
Assert-True ($null -ne $localization -and $null -ne $localization.SetMethod) 'GameSettingsManager.Localization setter is missing.'
Assert-True ($localization.PropertyType.FullName -eq 'UnityEngine.SystemLanguage') 'Localization type changed.'

$localizationType = $gameAssembly.GetType('Restory.Data.Localization.LocalizationSystem', $true)
$lookup = @($localizationType.GetMethods([Reflection.BindingFlags]'Public,NonPublic,Instance,Static') | Where-Object { $_.Name -eq 'GetTranslation' -and $_.ReturnType.FullName -eq 'System.String' -and $_.GetParameters().Count -eq 1 -and $_.GetParameters()[0].ParameterType.FullName -eq 'System.String' })
Assert-True ($lookup.Count -eq 1) 'Expected one-argument LocalizationSystem.GetTranslation(string) overload.'

$pluginType = $pluginAssembly.GetType('RestoryItalianPatch.RestoryItalianPlugin', $true)
$supportedPostfix = $pluginType.GetMethod('SupportedLocalizationsPostfix', [Reflection.BindingFlags]'Static,NonPublic')
Assert-True ($null -ne $supportedPostfix) 'SupportedLocalizations postfix is missing.'
$supportedParameter = $supportedPostfix.GetParameters()[0]
Assert-True ($supportedParameter.ParameterType.IsByRef) 'SupportedLocalizations postfix result is not by reference.'
Assert-True ($supportedParameter.ParameterType.GetElementType() -eq $languages.PropertyType) 'SupportedLocalizations postfix type does not match game API.'
$translationPostfix = $pluginType.GetMethod('GetTranslationPostfix', [Reflection.BindingFlags]'Static,NonPublic')
Assert-True ($null -ne $translationPostfix) 'GetTranslation postfix is missing.'
Assert-True ($translationPostfix.GetParameters()[1].ParameterType -eq [string].MakeByRefType()) 'GetTranslation postfix result is not ref string.'

Write-Output 'PASS: game localization targets and Harmony postfix signatures match the installed managed API.'
