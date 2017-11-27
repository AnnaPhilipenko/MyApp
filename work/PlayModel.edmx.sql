
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 06/08/2016 10:34:46
-- Generated from EDMX file: C:\Users\Евгений\Desktop\work\work\PlayModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [Play];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[playlist]', 'U') IS NOT NULL
    DROP TABLE [dbo].[playlist];
GO
IF OBJECT_ID(N'[dbo].[songs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[songs];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'playlist'
CREATE TABLE [dbo].[playlist] (
    [id] int IDENTITY(1,1) NOT NULL,
    [name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'songs'
CREATE TABLE [dbo].[songs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [playlist_id] int  NULL,
    [song] nvarchar(max)  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [id] in table 'playlist'
ALTER TABLE [dbo].[playlist]
ADD CONSTRAINT [PK_playlist]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [Id] in table 'songs'
ALTER TABLE [dbo].[songs]
ADD CONSTRAINT [PK_songs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [playlist_id] in table 'songs'
ALTER TABLE [dbo].[songs]
ADD CONSTRAINT [FK_playlistsongs]
    FOREIGN KEY ([playlist_id])
    REFERENCES [dbo].[playlist]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_playlistsongs'
CREATE INDEX [IX_FK_playlistsongs]
ON [dbo].[songs]
    ([playlist_id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------