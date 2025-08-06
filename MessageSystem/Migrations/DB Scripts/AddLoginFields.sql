ALTER TABLE [Users]
ADD username NVARCHAR(100) NOT NULL UNIQUE,
    password NVARCHAR(255) NOT NULL;

Create Index [IX_Users_username] ON [Users] (username);