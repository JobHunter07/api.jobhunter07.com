CREATE PROCEDURE dbo.GetTopMatchesForPosition
    @PositionID INT,
    @MaxResults INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Get Job Requirements first to keep the join scope small
    DECLARE @TargetTitleID INT, @TargetLevelRank INT;
    
    SELECT 
        @TargetTitleID = MasterTitleID,
        @TargetLevelRank = (SELECT OrdinalRank FROM JobLevels WHERE LevelID = p.LevelID)
    FROM Positions p
    WHERE p.PositionID = @PositionID;

    -- 2. Execute the Match
    SELECT TOP (@MaxResults)
        u.UserID,
        u.FullName,
        u.Email,
        mt.CanonicalName AS UserTitle,
        jl.LevelName AS UserLevel,
        -- Calculate Skill Match Percentage
        (SELECT COUNT(*) 
         FROM UserSkills us 
         JOIN PositionSkills ps ON us.SkillID = ps.SkillID 
         WHERE us.UserID = u.UserID AND ps.PositionID = @PositionID) AS MatchedSkillsCount,
        (SELECT COUNT(*) FROM PositionSkills WHERE PositionID = @PositionID) AS TotalRequiredSkills
    FROM Users u
    JOIN UserWorkProfiles uwp ON u.UserID = uwp.UserID
    JOIN MasterTitles mt ON uwp.MasterTitleID = mt.MasterTitleID
    JOIN JobLevels jl ON uwp.LevelID = jl.LevelID
    WHERE 
        uwp.MasterTitleID = @TargetTitleID -- The "Puzzle Piece" Title Match
        AND jl.OrdinalRank >= @TargetLevelRank -- Meets or exceeds seniority
        AND uwp.IsOpenToWork = 1
    ORDER BY 
        MatchedSkillsCount DESC, -- Best technical fit first
        jl.OrdinalRank ASC;       -- Then by closest seniority fit (don't over-hire)
END
GO