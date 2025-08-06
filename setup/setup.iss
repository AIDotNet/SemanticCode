; Inno Setup script for SemanticCode
#define MyAppVersion GetEnv("APP_VERSION")
#if MyAppVersion == ""
  #define MyAppVersion "0.1.7.0"
#endif

[Setup]
AppId={{8B9F4B7C-4A8D-4E6F-9B3C-2D5A7E8F1C4A}
AppName=SemanticCode
AppVersion={#MyAppVersion}
AppVerName=SemanticCode {#MyAppVersion}
AppPublisher=AIDotNet
AppPublisherURL=https://github.com/AIDotNet/SemanticCode
AppSupportURL=https://github.com/AIDotNet/SemanticCode/issues
AppUpdatesURL=https://github.com/AIDotNet/SemanticCode/releases
DefaultDirName={autopf}\SemanticCode
DisableProgramGroupPage=yes
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=../dist
OutputBaseFilename=SemanticCode-Setup-{#MyAppVersion}-win-x64
;SetupIconFile=../SemanticCode/Assets/favicon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
; Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1
Name: "associatefiles"; Description: "Associate .md files with SemanticCode"; GroupDescription: "File Associations"; Flags: unchecked

[Files]
Source: "../publish/win-x64/*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\SemanticCode"; Filename: "{app}\SemanticCode.Desktop.exe"
Name: "{autodesktop}\SemanticCode"; Filename: "{app}\SemanticCode.Desktop.exe"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\SemanticCode"; Filename: "{app}\SemanticCode.Desktop.exe"; Tasks: quicklaunchicon

[Registry]
Root: HKCR; Subkey: ".md"; ValueType: string; ValueName: ""; ValueData: "SemanticCodeFile"; Flags: uninsdeletevalue; Tasks: associatefiles
Root: HKCR; Subkey: "SemanticCodeFile"; ValueType: string; ValueName: ""; ValueData: "SemanticCode Markdown File"; Flags: uninsdeletekey; Tasks: associatefiles
Root: HKCR; Subkey: "SemanticCodeFile\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\SemanticCode.Desktop.exe,0"; Tasks: associatefiles
Root: HKCR; Subkey: "SemanticCodeFile\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\SemanticCode.Desktop.exe"" ""%1"""; Tasks: associatefiles

[Run]
Filename: "{app}\SemanticCode.Desktop.exe"; Description: "{cm:LaunchProgram,SemanticCode}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{userappdata}\SemanticCode"

[Code]
function GetUninstallString(): String;
var
  sUnInstPath: String;
  sUnInstallString: String;
begin
  sUnInstPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#SetupSetting("AppId")}_is1');
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
end;

function IsUpgrade(): Boolean;
begin
  Result := (GetUninstallString() <> '');
end;

function UnInstallOldVersion(): Integer;
var
  sUnInstallString: String;
  iResultCode: Integer;
begin
  Result := 0;
  sUnInstallString := GetUninstallString();
  if sUnInstallString <> '' then begin
    sUnInstallString := RemoveQuotes(sUnInstallString);
    if Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then
      Result := 3
    else
      Result := 2;
  end else
    Result := 1;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep=ssInstall) then
  begin
    if (IsUpgrade()) then
    begin
      UnInstallOldVersion();
    end;
  end;
end;