CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [UserName] nvarchar(max) NOT NULL,
    [Password] nvarchar(max) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
);
GO


CREATE TABLE [Messages] (
    [MessageId] int NOT NULL IDENTITY,
    [Text] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [SentByUserId] int NOT NULL,
    CONSTRAINT [PK_Messages] PRIMARY KEY ([MessageId]),
    CONSTRAINT [FK_Messages_Users_SentByUserId] FOREIGN KEY ([SentByUserId]) REFERENCES [Users] ([UserId])
);
GO


CREATE TABLE [Threads] (
    [ThreadId] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedByUserId] int NOT NULL,
    CONSTRAINT [PK_Threads] PRIMARY KEY ([ThreadId]),
    CONSTRAINT [FK_Threads_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([UserId])
);
GO


CREATE TABLE [ThreadToMessages] (
    [Id] int NOT NULL IDENTITY,
    [ThreadId] int NOT NULL,
    [MessageId] int NOT NULL,
    [Position] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ThreadToMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ThreadToMessages_Messages_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [Messages] ([MessageId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ThreadToMessages_Threads_ThreadId] FOREIGN KEY ([ThreadId]) REFERENCES [Threads] ([ThreadId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [ThreadToUser] (
    [Id] int NOT NULL IDENTITY,
    [Owner] bit NOT NULL,
    [ThreadId] int NOT NULL,
    [UserId] int NOT NULL,
    [LastReadPosition] int NOT NULL,
    CONSTRAINT [PK_ThreadToUser] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ThreadToUser_Threads_ThreadId] FOREIGN KEY ([ThreadId]) REFERENCES [Threads] ([ThreadId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ThreadToUser_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO


CREATE INDEX [IX_Messages_SentByUserId] ON [Messages] ([SentByUserId]);
GO


CREATE INDEX [IX_Threads_CreatedByUserId] ON [Threads] ([CreatedByUserId]);
GO


CREATE INDEX [IX_ThreadToMessages_MessageId] ON [ThreadToMessages] ([MessageId]);
GO


CREATE INDEX [IX_ThreadToMessages_ThreadId] ON [ThreadToMessages] ([ThreadId]);
GO


CREATE INDEX [IX_ThreadToUser_ThreadId] ON [ThreadToUser] ([ThreadId]);
GO


CREATE INDEX [IX_ThreadToUser_UserId] ON [ThreadToUser] ([UserId]);
GO


