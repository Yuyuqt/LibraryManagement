USE LibraryManagement;
GO

-- Add RewardId column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Memberships') AND name = 'RewardId')
BEGIN
    ALTER TABLE Memberships ADD RewardId NVARCHAR(50) NULL;
END
GO

-- Populate RewardId for existing memberships
UPDATE Memberships SET RewardId = '39aff010-693e-450e-85e7-ae6c3f1a2078' WHERE Type = 'Basic Monthly';
UPDATE Memberships SET RewardId = '94bc0b20-1a87-4e10-a778-dd48552470d0' WHERE Type = 'Basic Yearly';
GO
