using FarmExchange.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmExchange.Data
{
    public static class DbInitializer
    {
        public static void Initialize(FarmExchangeDbContext context)
        {
            // Ensure the database itself exists
            context.Database.EnsureCreated();

            // Manually add tables if they don't exist (because we can't run Migrations in this environment)

            // 1. ForumThreads
            var createThreadsTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ForumThreads' AND xtype='U')
                BEGIN
                    CREATE TABLE [ForumThreads] (
                        [Id] uniqueidentifier NOT NULL,
                        [AuthorId] uniqueidentifier NOT NULL,
                        [Title] nvarchar(200) NOT NULL,
                        [Content] nvarchar(max) NOT NULL,
                        [Category] nvarchar(50) NOT NULL DEFAULT 'General',
                        [CreatedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_ForumThreads] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_ForumThreads_Profiles_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Profiles] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_ForumThreads_Category] ON [ForumThreads] ([Category]);
                    CREATE INDEX [IX_ForumThreads_AuthorId] ON [ForumThreads] ([AuthorId]);
                END";
            context.Database.ExecuteSqlRaw(createThreadsTable);

            // 2. ForumPosts
            var createPostsTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ForumPosts' AND xtype='U')
                BEGIN
                    CREATE TABLE [ForumPosts] (
                        [Id] uniqueidentifier NOT NULL,
                        [ThreadId] uniqueidentifier NOT NULL,
                        [AuthorId] uniqueidentifier NOT NULL,
                        [Content] nvarchar(2000) NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_ForumPosts] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_ForumPosts_ForumThreads_ThreadId] FOREIGN KEY ([ThreadId]) REFERENCES [ForumThreads] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_ForumPosts_Profiles_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Profiles] ([Id]) ON DELETE NO ACTION
                    );
                    CREATE INDEX [IX_ForumPosts_ThreadId] ON [ForumPosts] ([ThreadId]);
                    CREATE INDEX [IX_ForumPosts_AuthorId] ON [ForumPosts] ([AuthorId]);
                END";
            context.Database.ExecuteSqlRaw(createPostsTable);

            // 4. UserAddresses
            var createUserAddressesTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserAddresses' AND xtype='U')
                BEGIN
                    CREATE TABLE [UserAddresses] (
                        [AddressID] int IDENTITY(1,1) NOT NULL,
                        [UserID] uniqueidentifier NOT NULL,
                        [UnitNumber] nvarchar(50) NULL,
                        [StreetName] nvarchar(100) NULL,
                        [Barangay] nvarchar(100) NULL,
                        [City] nvarchar(100) NULL,
                        [Province] nvarchar(100) NULL,
                        [PostalCode] nvarchar(10) NULL,
                        [Country] nvarchar(50) DEFAULT 'Philippines',
                        CONSTRAINT [PK_UserAddresses] PRIMARY KEY ([AddressID]),
                        CONSTRAINT [FK_UserAddresses_Profiles_UserID] FOREIGN KEY ([UserID]) REFERENCES [Profiles] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_UserAddresses_UserID] ON [UserAddresses] ([UserID]);
                END";
            context.Database.ExecuteSqlRaw(createUserAddressesTable);

            // 4.1 Update UserAddresses to add Region if not exists
            var alterUserAddressesTable = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'Region' AND Object_ID = Object_ID(N'UserAddresses'))
                BEGIN
                    ALTER TABLE [UserAddresses] ADD [Region] nvarchar(100) NULL;
                END";
            context.Database.ExecuteSqlRaw(alterUserAddressesTable);
        }
    }
}
