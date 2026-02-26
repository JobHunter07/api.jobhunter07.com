CREATE PROCEDURE dbo.GetSmartMatchesForPosition
    @PositionID INT
AS
BEGIN
    DECLARE @JobLoc GEOGRAPHY, @IsRemote BIT, @TitleID INT, @MinRank INT;

    -- Get Job context
    SELECT 
        @JobLoc = c.Location, 
        @IsRemote = p.IsRemote,
        @TitleID = p.MasterTitleID,
        @MinRank = (SELECT OrdinalRank FROM JobLevels WHERE LevelID = p.LevelID)
    FROM Positions p
    JOIN Companies c ON p.CompanyID = c.CompanyID
    WHERE p.PositionID = @PositionID;

    SELECT 
        u.FullName,
        -- Convert meters to miles (1609.34 meters = 1 mile)
        ROUND(@JobLoc.STDistance(uwp.HomeLocation) / 1609.34, 1) AS DistanceMiles,
        (SELECT COUNT(*) FROM UserSkills us 
         JOIN PositionSkills ps ON us.SkillID = ps.SkillID 
         WHERE us.UserID = u.UserID AND ps.PositionID = @PositionID) AS SkillMatchCount
    FROM Users u
    JOIN UserWorkProfiles uwp ON u.UserID = uwp.UserID
    JOIN JobLevels jl ON uwp.LevelID = jl.LevelID
    WHERE 
        uwp.MasterTitleID = @TitleID
        AND jl.OrdinalRank >= @MinRank
        AND (
            @IsRemote = 1 OR -- If job is remote, location doesn't matter
            uwp.PrefersRemote = 0 AND (@JobLoc.STDistance(uwp.HomeLocation) / 1609.34) <= uwp.MaxCommuteMiles
        )
    ORDER BY SkillMatchCount DESC, DistanceMiles ASC;
END
GO