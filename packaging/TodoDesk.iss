#define MyAppName "Todo Desk"
#define MyAppPublisher "Todo Desk"

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

#ifndef SourceRoot
  #define SourceRoot ".."
#endif

#ifndef StageDir
  #define StageDir SourceRoot + "\build\stage"
#endif

[Setup]
AppId={{C9F3AE64-AC64-44C5-89E4-DF5CF6E4FC36}
AppName={#MyAppName}
AppVersion={#AppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\Todo Desk
DefaultGroupName=Todo Desk
DisableProgramGroupPage=yes
OutputDir={#SourceRoot}\build\installer
OutputBaseFilename=TodoDesk-Setup-{#AppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayName=Todo Desk
SetupIconFile={#StageDir}\app.ico
UninstallDisplayIcon={app}\TodoDesk.exe

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "{#StageDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\Todo Desk"; Filename: "{app}\TodoDesk.exe"
Name: "{autodesktop}\Todo Desk"; Filename: "{app}\TodoDesk.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\TodoDesk.exe"; Description: "Launch Todo Desk"; Flags: nowait postinstall skipifsilent
