#ifndef AppName
  #define AppName "MultiCommentViewer"
#endif
#ifndef AppPublisher
  #define AppPublisher "ryu-s"
#endif
#ifndef AppExeName
  #define AppExeName "MultiCommentViewer.exe"
#endif
#ifndef AppVersion
  #define AppVersion "0.1.0"
#endif
#ifndef OutputBaseFilename
  #define OutputBaseFilename "MultiCommentViewer-Setup"
#endif
#ifndef CoreZipPath
  #define CoreZipPath "..\..\output\MultiCommentViewer_v0.1.0_beta.zip"
#endif
#ifndef BasicPluginsDir
  #define BasicPluginsDir "..\..\output\plugins\basic"
#endif

[Setup]
AppId={{8E2E9E8E-97E3-4D88-ACAB-3AE4F31B08D7}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={localappdata}\Programs\MultiCommentViewer
DefaultGroupName={#AppName}
OutputDir=..\..\output
OutputBaseFilename={#OutputBaseFilename}
Compression=lzma
SolidCompression=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#AppExeName}

[Languages]
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "desktopicon"; Description: "デスクトップアイコンを作成する"; GroupDescription: "追加タスク:"; Flags: unchecked
Name: "installbasicplugins"; Description: "基本プラグインをインストールする"; GroupDescription: "プラグイン:"; Flags: checkedonce

[Files]
; mcv本体ZIP（同梱してインストール時に展開）
Source: "{#CoreZipPath}"; DestDir: "{tmp}"; DestName: "core_payload.zip"; Flags: ignoreversion deleteafterinstall
; 基本プラグイン（DLL）を同梱インストール
Source: "{#BasicPluginsDir}\*.dll"; DestDir: "{app}\plugins"; Tasks: installbasicplugins; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{#AppName} を起動"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; インストーラで配置したキャッシュのみ削除（ユーザーデータは保持）
Type: filesandordirs; Name: "{app}\cache"

[Code]
procedure ExpandCoreZipOrFail();
var
  PsExe: string;
  PsArgs: string;
  ResultCode: Integer;
begin
  ForceDirectories(ExpandConstant('{app}'));

  PsExe := ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe');
  PsArgs :=
    '-NoProfile -ExecutionPolicy Bypass -Command "Expand-Archive -Path ''' +
    ExpandConstant('{tmp}\core_payload.zip') +
    ''' -DestinationPath ''' +
    ExpandConstant('{app}') +
    ''' -Force"';

  if not Exec(PsExe, PsArgs, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
    RaiseException('MCV本体ZIPの展開実行に失敗しました。');

  if ResultCode <> 0 then
    RaiseException(Format('MCV本体ZIPの展開に失敗しました。(ExitCode=%d)', [ResultCode]));
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
    ExpandCoreZipOrFail();
end;
