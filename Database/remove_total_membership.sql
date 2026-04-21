-- EXTREMELY AGGRESSIVE script to remove TotalMembership and ALL its dependencies
USE LibraryManagement;
GO

PRINT 'Starting aggressive cleanup of TotalMembership column...';

-- 1. Specifically drop the constraint the user mentioned if it exists
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'DF__UserSubsc__Total__5DCAEF64' AND type = 'D')
BEGIN
    PRINT 'Explicitly dropping known constraint DF__UserSubsc__Total__5DCAEF64';
    ALTER TABLE UserSubscriptions DROP CONSTRAINT DF__UserSubsc__Total__5DCAEF64;
END

-- 2. Dynamically find and drop ALL other default constraints on this column
DECLARE @SQL nvarchar(MAX) = '';
SELECT @SQL += 'ALTER TABLE UserSubscriptions DROP CONSTRAINT ' + d.name + ';' + CHAR(13)
FROM sys.default_constraints d
INNER JOIN sys.columns c ON d.parent_column_id = c.column_id AND d.parent_object_id = c.object_id
WHERE c.object_id = OBJECT_ID('UserSubscriptions')
AND c.name = 'TotalMembership';

IF @SQL <> ''
BEGIN
    PRINT 'Dropping additional dynamic constraints...';
    EXEC sp_executesql @SQL;
END

-- 3. Find and drop any Indexes that use this column
SET @SQL = '';
SELECT @SQL += 'DROP INDEX ' + i.name + ' ON UserSubscriptions;' + CHAR(13)
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('UserSubscriptions')
AND c.name = 'TotalMembership';

IF @SQL <> ''
BEGIN
    PRINT 'Dropping dependent indexes...';
    EXEC sp_executesql @SQL;
END

-- 4. Check for and drop foreign keys if they somehow depend on this (rare for this name but possible)
SET @SQL = '';
SELECT @SQL += 'ALTER TABLE UserSubscriptions DROP CONSTRAINT ' + fk.name + ';' + CHAR(13)
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
WHERE fk.parent_object_id = OBJECT_ID('UserSubscriptions')
AND c.name = 'TotalMembership';

IF @SQL <> ''
BEGIN
    PRINT 'Dropping dependent foreign keys...';
    EXEC sp_executesql @SQL;
END

-- 5. Finally, drop the column
IF EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID('UserSubscriptions') 
    AND name = 'TotalMembership'
)
BEGIN
    ALTER TABLE UserSubscriptions DROP COLUMN TotalMembership;
    PRINT 'SUCCESS: Dropped column TotalMembership from UserSubscriptions.';
END
ELSE
BEGIN
    PRINT 'Done: Column TotalMembership does not exist in UserSubscriptions.';
END
GO
