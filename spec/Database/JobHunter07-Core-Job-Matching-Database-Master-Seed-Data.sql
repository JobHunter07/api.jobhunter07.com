/*******************************************************************************
   SEED DATA SCRIPT: Work Profile & Job Matching
   Target: 10 Companies, 200 Positions, 200 Users (25% Match Rate for .NET)
*******************************************************************************/

-- 1. SEED MASTER DATA
--------------------------------------------------------------------------------
INSERT INTO JobFamilies (FamilyName) VALUES ('Engineering'), ('Product'), ('Design'), ('Marketing');

INSERT INTO MasterTitles (JobFamilyID, CanonicalName, SOC_Code) VALUES 
(1, '.NET Developer', '15-1252.00'),
(1, 'Frontend Engineer', '15-1254.00'),
(1, 'DevOps Engineer', '15-1299.08'),
(2, 'Product Manager', '11-3021.00');

INSERT INTO TitleAliases (MasterTitleID, AliasName) VALUES 
(1, 'C# Developer'), (1, 'Dotnet Engineer'), (1, '.NET C# Software Engineer'),
(2, 'React Developer'), (3, 'Site Reliability Engineer');

INSERT INTO JobLevels (LevelCode, LevelName, OrdinalRank) VALUES 
('L1', 'Junior', 1), ('L2', 'Mid-Level', 2), ('L3', 'Senior', 3), ('L4', 'Staff', 4);

INSERT INTO JobPayMatrix (MasterTitleID, LevelID, HourlyRate, AnnualSalary) VALUES 
(1, 1, 45.00, 90000), (1, 2, 60.00, 120000), (1, 3, 75.00, 150000), (1, 4, 95.00, 190000);

INSERT INTO MasterSkills (SkillName) VALUES 
('C#'), ('.NET Core'), ('SQL Server'), ('React'), ('AWS'), ('Docker'), ('TypeScript');

-- 2. SEED COMPANIES & POSITIONS
--------------------------------------------------------------------------------
-- Create 10 Companies
DECLARE @i INT = 1;
WHILE @i <= 10 BEGIN
    INSERT INTO Companies (CompanyName) VALUES (CONCAT('TechCorp ', @i));
    SET @i = @i + 1;
END

-- Create 200 Positions (20 per company)
-- 20 of these will be '.NET C#' related (MasterTitleID = 1)
-- 3-5 per company will be set to 'Hiring'
INSERT INTO Positions (CompanyID, MasterTitleID, LevelID, JobStatus, RemoteType, City, CountryCode)
SELECT 
    c.CompanyID,
    CASE WHEN p.n <= 2 THEN 1 ELSE (p.n % 3) + 2 END, -- First 2 per company are .NET
    (p.n % 4) + 1,
    CASE WHEN p.n <= 4 THEN 'Hiring' ELSE 'Active' END, -- 4 Hiring per company
    'Hybrid', 'Berlin', 'DE'
FROM Companies c
CROSS JOIN (SELECT TOP 20 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) n FROM sys.objects) p;

-- Link Skills to .NET Positions
INSERT INTO PositionSkills (PositionID, SkillID)
SELECT PositionID, s.SkillID
FROM Positions p
CROSS JOIN (SELECT SkillID FROM MasterSkills WHERE SkillName IN ('C#', '.NET Core', 'SQL Server')) s
WHERE p.MasterTitleID = 1;

-- 3. SEED USERS & WORK PROFILES (Matching 25%)
--------------------------------------------------------------------------------
-- Create 200 Users
SET @i = 1;
WHILE @i <= 200 BEGIN
    INSERT INTO Users (Email, FullName) 
    VALUES (CONCAT('user', @i, '@example.com'), CONCAT('Candidate ', @i));
    
    -- Assign Work Profiles
    -- We want 50 Users (25%) to match MasterTitleID 1 ( .NET )
    INSERT INTO UserWorkProfiles (UserID, MasterTitleID, LevelID, CurrentSalary)
    VALUES (
        SCOPE_IDENTITY(), 
        CASE WHEN @i <= 50 THEN 1 ELSE (@i % 3) + 2 END, 
        (@i % 4) + 1, 
        100000 + (@i * 100)
    );

    -- Give the first 50 users the required .NET skills for a match
    IF @i <= 50 BEGIN
        INSERT INTO UserSkills (UserID, SkillID, YearsOfExperience)
        SELECT SCOPE_IDENTITY(), SkillID, 5 FROM MasterSkills WHERE SkillName IN ('C#', '.NET Core');
    END

    SET @i = @i + 1;
END