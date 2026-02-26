CREATE PROCEDURE dbo.GetUserFullWorkProfile
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if user exists first to save resources
    IF NOT EXISTS (SELECT 1 FROM Users WHERE UserID = @UserID)
    BEGIN
        RAISERROR('User not found', 16, 1);
        RETURN;
    END

    SELECT 
        -- Basic Info
        u.FullName AS [basics.name],
        u.Email AS [basics.email],
        u.Summary AS [basics.summary],
        uwp.CurrentSalary AS [basics.salary],
        
        -- Master Title & Level info
        mt.CanonicalName AS [basics.label],
        jl.LevelName AS [basics.level],

        -- Nested Skills Array
        (SELECT 
            ms.SkillName AS [name],
            us.YearsOfExperience AS [level] -- Mapping years to level for schema match
         FROM UserSkills us
         JOIN MasterSkills ms ON us.SkillID = ms.SkillID
         WHERE us.UserID = u.UserID
         FOR JSON PATH) AS [skills],

        -- Nested Work Experience Array
        (SELECT 
            we.CompanyName AS [name],
            we.Position AS [position],
            we.StartDate AS [startDate],
            we.EndDate AS [endDate],
            we.Summary AS [summary]
         FROM WorkExperience we
         WHERE we.UserID = u.UserID
         FOR JSON PATH) AS [work]

    FROM Users u
    LEFT JOIN UserWorkProfiles uwp ON u.UserID = uwp.UserID
    LEFT JOIN MasterTitles mt ON uwp.MasterTitleID = mt.MasterTitleID
    LEFT JOIN JobLevels jl ON uwp.LevelID = jl.LevelID
    WHERE u.UserID = @UserID
    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER; -- Returns a single JSON object instead of a list
END
GO