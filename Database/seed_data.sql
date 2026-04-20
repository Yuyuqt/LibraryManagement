-- Seed Data for Library Management System
USE LibraryManagement;
GO

-- 1. Insert Categories
SET IDENTITY_INSERT Categories ON;
INSERT INTO Categories (Id, Name) VALUES 
(1, 'Fiction'),
(2, 'Science'),
(3, 'History'),
(4, 'Biography'),
(5, 'Technology'),
(6, 'Philosophy'),
(7, 'Self-Help'),
(8, 'Fantasy'),
(9, 'Mystery'),
(10, 'Business');
SET IDENTITY_INSERT Categories OFF;
GO

-- 2. Insert Memberships
SET IDENTITY_INSERT Memberships ON;
INSERT INTO Memberships (Id, Type, MaxBooks, BorrowingDays, Price, DurationMonths) VALUES 
(1, 'Basic', 2, 7, 10.00, 1),
(2, 'Standard', 5, 14, 25.00, 3),
(3, 'Premium', 10, 30, 80.00, 12),
(4, 'Student', 3, 15, 15.00, 6);
SET IDENTITY_INSERT Memberships OFF;
GO

-- 3. Insert Users (Password is 'Password123' hashed typically, but using placeholder for SQL)
SET IDENTITY_INSERT Users ON;
INSERT INTO Users (Id, FullName, Email, PasswordHash, PhoneNumber, Role, IsActive, CreatedAt, Address) VALUES 
(1, 'Admin User', 'admin@library.com', 'AQAAAAEAACcQAAAAE...', '09123456789', 'Admin', 1, GETDATE(), '123 Main St, Central City'),
(2, 'John Doe', 'john.doe@gmail.com', 'AQAAAAEAACcQAAAAE...', '09234567890', 'Member', 1, GETDATE(), '456 Oak Ave, North Side'),
(3, 'Jane Smith', 'jane.smith@yahoo.com', 'AQAAAAEAACcQAAAAE...', '09345678901', 'Member', 1, GETDATE(), '789 Pine Rd, South Village'),
(4, 'David Wilson', 'david.w@university.edu', 'AQAAAAEAACcQAAAAE...', '09456789012', 'Member', 1, GETDATE(), '101 University Dr, Campus'),
(5, 'Sarah Miller', 'sarah.m@gmail.com', 'AQAAAAEAACcQAAAAE...', '09567890123', 'Member', 1, GETDATE(), '202 Lake View, West Coast');
SET IDENTITY_INSERT Users OFF;
GO

-- 4. Insert Books
SET IDENTITY_INSERT Books ON;
INSERT INTO Books (Id, Title, ISBN, Author, Status, IsActive, CreatedAt, Description, TotalCopies, AvailableCopies) VALUES 
(1, 'The Great Gatsby', '9780743273565', 'F. Scott Fitzgerald', 'Available', 1, GETDATE(), 'A classic novel of the Jazz Age.', 5, 5),
(2, 'A Brief History of Time', '9780553380163', 'Stephen Hawking', 'Available', 1, GETDATE(), 'Exploring the origin and fate of the universe.', 3, 3),
(3, 'Steve Jobs', '9781451648539', 'Walter Isaacson', 'Available', 1, GETDATE(), 'The exclusive biography of Steve Jobs.', 2, 2),
(4, 'The Pragmatic Programmer', '9780135957059', 'Andrew Hunt', 'Available', 1, GETDATE(), 'Your journey to mastery in software development.', 4, 4),
(5, 'Sapiens', '9780062316097', 'Yuval Noah Harari', 'Available', 1, GETDATE(), 'A brief history of humankind.', 6, 6),
(6, 'The Hobbit', '9780547928227', 'J.R.R. Tolkien', 'Available', 1, GETDATE(), 'A prelude to The Lord of the Rings.', 8, 8),
(7, 'Atomic Habits', '9780735211292', 'James Clear', 'Available', 1, GETDATE(), 'An easy & proven way to build good habits.', 10, 10),
(8, 'The Da Vinci Code', '9780307474278', 'Dan Brown', 'Available', 1, GETDATE(), 'A murder in the Louvre leads to a complex mystery.', 5, 5),
(9, 'Lean In', '9780385349949', 'Sheryl Sandberg', 'Available', 1, GETDATE(), 'Women, work, and the will to lead.', 3, 3),
(10, 'Clean Code', '9780132350884', 'Robert C. Martin', 'Available', 1, GETDATE(), 'A handbook of agile software craftsmanship.', 4, 4);
SET IDENTITY_INSERT Books OFF;
GO

-- 5. Book-Category Mapping
INSERT INTO BookCategories (BookId, CategoryId) VALUES 
(1, 1), -- Great Gatsby - Fiction
(2, 2), -- Brief History - Science
(3, 4), -- Steve Jobs - Biography
(4, 5), -- Pragmatic Programmer - Technology
(5, 3), -- Sapiens - History
(6, 8), -- Hobbit - Fantasy
(7, 7), -- Atomic Habits - Self-Help
(8, 9), -- Da Vinci Code - Mystery
(9, 10), -- Lean In - Business
(10, 5); -- Clean Code - Technology
GO

-- 6. User Subscriptions
INSERT INTO UserSubscriptions (UserId, MembershipId, StartDate, ExpiryDate, IsActive) VALUES 
(2, 2, '2026-01-01', '2026-04-01', 1),
(3, 3, '2026-01-01', '2027-01-01', 1),
(4, 4, '2026-02-15', '2026-08-15', 1),
(5, 1, '2026-04-01', '2026-05-01', 1);
GO

-- 7. Some Borrowing History
INSERT INTO Borrowings (UserId, BookId, BorrowDate, DueDate, ReturnDate, Status) VALUES 
(2, 1, '2026-04-10', '2026-04-24', '2026-04-15', 'Returned'),
(2, 4, '2026-04-16', '2026-04-30', NULL, 'Active'),
(3, 7, '2026-04-01', '2026-05-01', NULL, 'Active'),
(4, 10, '2026-04-18', '2026-05-03', NULL, 'Active');
GO
