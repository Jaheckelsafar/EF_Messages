CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Messages] (
    [Id] int NOT NULL IDENTITY,
    [Text] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [SentByUserId] int NOT NULL,
    CONSTRAINT [PK_Messages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Messages_Users_SentByUserId] FOREIGN KEY ([SentByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO