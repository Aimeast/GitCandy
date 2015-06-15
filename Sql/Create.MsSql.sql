CREATE TABLE [Users](
  [ID] BigInt PRIMARY KEY IDENTITY(1,1) NOT NULL,
  [Name] NVarChar(20) COLLATE Latin1_General_CI_AS NOT NULL,
  [Nickname] NVarChar(20) NOT NULL,
  [Email] NVarChar(50) NOT NULL,
  [PasswordVersion] Int NOT NULL,
  [Password] Char(32) NOT NULL,
  [Description] NVarChar(500) NOT NULL,
  [IsSystemAdministrator] Bit NOT NULL,
  [CreationDate] Datetime NOT NULL
);

CREATE TABLE [Teams](
  [ID] BigInt PRIMARY KEY IDENTITY(1,1) NOT NULL,
  [Name] NVarChar(20) COLLATE Latin1_General_CI_AS NOT NULL,
  [Description] NVarChar(500) NOT NULL,
  [CreationDate] Datetime NOT NULL
);

CREATE TABLE [Repositories](
  [ID] BigInt PRIMARY KEY IDENTITY(1,1) NOT NULL,
  [Name] NVarChar(50) COLLATE Latin1_General_CI_AS NOT NULL,
  [Description] NVarChar(500) NOT NULL,
  [CreationDate] Datetime NOT NULL,
  [IsPrivate] Bit NOT NULL,
  [AllowAnonymousRead] Bit NOT NULL,
  [AllowAnonymousWrite] Bit NOT NULL
);

CREATE TABLE [UserTeamRole](
  [UserID] BigInt NOT NULL,
  [TeamID] BigInt NOT NULL,
  [IsAdministrator] Bit NOT NULL,
  Constraint [UNQ_User_Team] Unique ([UserID], [TeamID]),
  Foreign Key ([UserID]) References [Users]([ID]),
  Foreign Key ([TeamID]) References [Teams]([ID])
);

CREATE TABLE [UserRepositoryRole](
  [UserID] BigInt NOT NULL,
  [RepositoryID] BigInt NOT NULL,
  [AllowRead] Bit NOT NULL,
  [AllowWrite] Bit NOT NULL,
  [IsOwner] Bit NOT NULL,
  Constraint [UNQ_User_Repository] Unique ([UserID], [RepositoryID]),
  Foreign Key ([UserID]) References [Users]([ID]),
  Foreign Key ([RepositoryID]) References [Repositories]([ID])
);

CREATE TABLE [TeamRepositoryRole](
  [TeamID] BigInt NOT NULL,
  [RepositoryID] BigInt NOT NULL,
  [AllowRead] Bit NOT NULL,
  [AllowWrite] Bit NOT NULL,
  Constraint [UNQ_Team_Repository] Unique ([TeamID], [RepositoryID]),
  Foreign Key ([TeamID]) References [Teams]([ID]),
  Foreign Key ([RepositoryID]) References [Repositories]([ID])
);

CREATE TABLE [AuthorizationLog] (
  [AuthCode] UniqueIdentifier PRIMARY KEY NOT NULL,
  [UserID] BigInt NOT NULL,
  [IssueDate] Datetime NOT NULL,
  [Expires] Datetime NOT NULL,
  [IssueIp] VarChar(40) NOT NULL,
  [LastIp] VarChar(40) NOT NULL,
  [IsValid] Bit NOT NULL,
  Foreign Key ([UserID]) References [Users]([ID])
);

CREATE TABLE [SshKeys] (
  [ID] BigInt PRIMARY KEY IDENTITY(1,1) NOT NULL,
  [UserID] BigInt NOT NULL,
  [KeyType] VarChar(20) NOT NULL,
  [Fingerprint] Char(47) NOT NULL,
  [PublicKey] VarChar(600) NOT NULL,
  [ImportData] Datetime NOT NULL,
  [LastUse] Datetime NOT NULL,
  Foreign Key ([UserID]) References [Users]([ID])
);

CREATE UNIQUE INDEX [Users_IX_User_Email] ON [Users] ([Name] ASC);
CREATE UNIQUE INDEX [Users_IX_User_Name] ON [Users] ([Email] ASC);
CREATE UNIQUE INDEX [Teams_IX_Team_Name] ON [Teams] ([Name] ASC);
CREATE UNIQUE INDEX [Repositories_IX_Repository_Name] ON [Repositories] ([Name] ASC);

-- Create an administrator user with admin:gitcandy
INSERT INTO [Users] VALUES ('admin', 'admin', 'admin@GitCandy', 1, '6BBBDB60C90AD35F944A934B6E83ABDC', 'System administrator', 1, GetDate())
