SELECT 
    mt.CanonicalName, 
    jl.LevelName, 
    COUNT(*) as CandidateCount
FROM UserWorkProfiles uwp
JOIN MasterTitles mt ON uwp.MasterTitleID = mt.MasterTitleID
JOIN JobLevels jl ON uwp.LevelID = jl.LevelID
WHERE mt.CanonicalName = '.NET Developer'
GROUP BY mt.CanonicalName, jl.LevelName;


-- Find a .NET Position that is currently 'Hiring'
DECLARE @TestJobID INT = (SELECT TOP 1 PositionID FROM Positions WHERE MasterTitleID = 1 AND JobStatus = 'Hiring');

-- Run the matcher
EXEC dbo.GetTopMatchesForPosition @PositionID = @TestJobID;