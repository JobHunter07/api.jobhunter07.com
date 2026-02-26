/*******************************************************************************
   MASTER SCHEMA: Work Profile & Job Matching System
   Scale Target: 1M+ Users
   Design Principle: Normalize strings into IDs for lightning-fast matching.
*******************************************************************************/

-- 1. MASTER DATA TABLES (The "Standard")
--------------------------------------------------------------------------------

-- Master list of categories (e.g., Engineering, Marketing)
CREATE TABLE JobFamilies (
    JobFamilyID INT IDENTITY(1,1) PRIMARY KEY,
    FamilyName NVARCHAR(100) NOT NULL UNIQUE
);

-- The "Source of Truth" for Job Titles. Links to VOC/O*NET standards.
CREATE TABLE MasterTitles (
    MasterTitleID INT IDENTITY(1,1) PRIMARY KEY,
    JobFamilyID INT REFERENCES JobFamilies(JobFamilyID),
    CanonicalName NVARCHAR(150) NOT NULL,
    SOC_Code VARCHAR(20), -- Standard Occupational Classification
    INDEX IX_MasterTitle_Name (CanonicalName)
);

-- AKA Titles: Maps "C# Ninja" to ".NET Developer"
CREATE TABLE TitleAliases (
    AliasID INT IDENTITY(1,1) PRIMARY KEY,
    MasterTitleID INT NOT NULL REFERENCES MasterTitles(MasterTitleID),
    AliasName NVARCHAR(150) NOT NULL,
    -- Unique index makes "lookups-by-string" near-instant
    CONSTRAINT UQ_AliasName UNIQUE (AliasName)
);

-- Leveling Matrix (The seniority component)
CREATE TABLE JobLevels (
    LevelID INT IDENTITY(1,1) PRIMARY KEY,
    LevelCode VARCHAR(10) NOT NULL, -- IC1, M2, etc.
    LevelName NVARCHAR(50) NOT NULL, -- Junior, Senior, Principal
    OrdinalRank INT NOT NULL -- Higher number = higher seniority for "Match" logic
);

-- The Pay Matrix: Links Title + Level to Money
CREATE TABLE JobPayMatrix (
    PayMatrixID INT IDENTITY(1,1) PRIMARY KEY,
    MasterTitleID INT NOT NULL REFERENCES MasterTitles(MasterTitleID),
    LevelID INT NOT NULL REFERENCES JobLevels(LevelID),
    HourlyRate DECIMAL(10,2),
    AnnualSalary DECIMAL(18,2),
    -- Prevent duplicate pay scales for the same title/level combo
    CONSTRAINT UQ_Title_Level_Pay UNIQUE (MasterTitleID, LevelID)
);

-- Master Skills list (Avoid storing "JavaScript" as a string a million times)
CREATE TABLE MasterSkills (
    SkillID INT IDENTITY(1,1) PRIMARY KEY,
    SkillName NVARCHAR(100) NOT NULL UNIQUE
);

-- 2. WORK PROFILE (The User Side)
--------------------------------------------------------------------------------

CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Email VARCHAR(255) NOT NULL UNIQUE, -- Indexing VARCHAR is faster than NVARCHAR
    FullName NVARCHAR(200) NOT NULL,
    Summary NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- The "Work Profile" links a User to the Standard Job Title and Level
CREATE TABLE UserWorkProfiles (
    UserID INT PRIMARY KEY REFERENCES Users(UserID),
    MasterTitleID INT REFERENCES MasterTitles(MasterTitleID),
    LevelID INT REFERENCES JobLevels(LevelID),
    CurrentSalary DECIMAL(18,2),
    IsOpenToWork BIT DEFAULT 1
);

-- User Skills (The "Puzzle Piece" for matching)
CREATE TABLE UserSkills (
    UserID INT NOT NULL REFERENCES Users(UserID),
    SkillID INT NOT NULL REFERENCES MasterSkills(SkillID),
    YearsOfExperience INT,
    PRIMARY KEY (UserID, SkillID) -- Composite PK ensures data integrity & speeds up JOINs
);

-- 3. POSITIONS (The Job Side)
--------------------------------------------------------------------------------

CREATE TABLE Companies (
    CompanyID INT IDENTITY(1,1) PRIMARY KEY,
    CompanyName NVARCHAR(200) NOT NULL
);

CREATE TABLE Positions (
    PositionID INT IDENTITY(1,1) PRIMARY KEY,
    CompanyID INT NOT NULL REFERENCES Companies(CompanyID),
    MasterTitleID INT NOT NULL REFERENCES MasterTitles(MasterTitleID),
    LevelID INT NOT NULL REFERENCES JobLevels(LevelID),
    JobStatus VARCHAR(20) DEFAULT 'Active', -- 'Hiring', 'Filled', 'Inactive'
    RemoteType VARCHAR(20), -- 'Onsite', 'Remote', 'Hybrid'
    City NVARCHAR(100),
    CountryCode CHAR(2),
    -- Optimization: Filtered index for active hiring
    INDEX IX_HiringStatus (JobStatus) WHERE JobStatus = 'Hiring'
);

-- Skills required for the specific position
CREATE TABLE PositionSkills (
    PositionID INT NOT NULL REFERENCES Positions(PositionID),
    SkillID INT NOT NULL REFERENCES MasterSkills(SkillID),
    IsRequired BIT DEFAULT 1, -- Mandatory vs Preferred
    PRIMARY KEY (PositionID, SkillID)
);

GO

/*******************************************************************************
   TIPS FOR HIGH PERFORMANCE AT 1M+ ROWS:
   1. Data Types: Use INT/BIGINT for Foreign Keys. String comparisons slow down at scale.
   2. Narrow Tables: Keep NVARCHAR(MAX) columns in separate "Detail" tables if you don't 
      need them for the initial search/match.
   3. Indexing: The PRIMARY KEY (UserID, SkillID) on UserSkills allows you to find 
      "Which users have Skill X" very quickly.
   4. Pay Matching: Matching (UserWorkProfile.MasterTitleID = Positions.MasterTitleID) 
      is O(log N) complexity, making it extremely fast.
*******************************************************************************/