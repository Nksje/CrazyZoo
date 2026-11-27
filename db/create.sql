-- Crazy Zoo: andmebaasi ja tabelite loomine LocalDB-s
-- Käivitamine: sqlcmd -S "(localdb)\MSSQLLocalDB" -i db\create.sql

IF DB_ID('CrazyZoo') IS NULL
    CREATE DATABASE CrazyZoo;
GO

USE CrazyZoo;
GO

-- Üks rida = üks salvestatud maailm (nimi, nt "default")
IF OBJECT_ID('dbo.Worlds') IS NULL
CREATE TABLE dbo.Worlds (
    Id       INT IDENTITY(1,1) PRIMARY KEY,
    Name     NVARCHAR(100) NOT NULL UNIQUE,
    Version  INT NOT NULL,
    SavedAt  DATETIME2 NOT NULL
);
GO

-- Rühmad (hoidlad). Id on rühma ID maailma sees.
IF OBJECT_ID('dbo.AnimalGroups') IS NULL
CREATE TABLE dbo.AnimalGroups (
    WorldId    INT NOT NULL REFERENCES dbo.Worlds(Id),
    Id         INT NOT NULL,
    Name       NVARCHAR(50) NOT NULL,
    AnimalType NVARCHAR(20) NOT NULL,
    CONSTRAINT PK_AnimalGroups PRIMARY KEY (WorldId, Id)
);
GO

-- Loomad. GroupId on välisvõti rühmale (rühmakuuluvus).
IF OBJECT_ID('dbo.Animals') IS NULL
CREATE TABLE dbo.Animals (
    WorldId       INT NOT NULL,
    Id            INT NOT NULL,
    Type          NVARCHAR(20) NOT NULL,
    Name          NVARCHAR(20) NOT NULL,
    Energy        INT NOT NULL CHECK (Energy BETWEEN 0 AND 100),
    Hunger        INT NOT NULL CHECK (Hunger BETWEEN 0 AND 100),
    Mood          INT NOT NULL CHECK (Mood BETWEEN 0 AND 100),
    ReactionCount INT NOT NULL CHECK (ReactionCount >= 0),
    GroupId       INT NOT NULL,
    CONSTRAINT PK_Animals PRIMARY KEY (WorldId, Id),
    CONSTRAINT FK_Animals_Groups FOREIGN KEY (WorldId, GroupId)
        REFERENCES dbo.AnimalGroups(WorldId, Id)
);
GO
