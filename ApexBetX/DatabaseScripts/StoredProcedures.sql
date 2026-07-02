USE ApexBetXDB;
GO

-- INSERT USER
CREATE OR ALTER PROCEDURE sp_InsertUser
    @IDNumber NVARCHAR(50),
    @FirstName NVARCHAR(100),
    @Surname NVARCHAR(100),
    @Email NVARCHAR(100),
    @Phone NVARCHAR(50)
AS
BEGIN
    INSERT INTO Users (IDNumber, FirstName, Surname, Email, Phone)
    VALUES (@IDNumber, @FirstName, @Surname, @Email, @Phone);
END
GO

-- UPDATE USER
CREATE OR ALTER PROCEDURE sp_UpdateUser
    @UserId INT,
    @IDNumber NVARCHAR(50),
    @FirstName NVARCHAR(100),
    @Surname NVARCHAR(100),
    @Email NVARCHAR(100),
    @Phone NVARCHAR(50)
AS
BEGIN
    UPDATE Users
    SET IDNumber = @IDNumber,
        FirstName = @FirstName,
        Surname = @Surname,
        Email = @Email,
        Phone = @Phone
    WHERE UserId = @UserId;
END
GO

-- DELETE USER
CREATE OR ALTER PROCEDURE sp_DeleteUser
    @UserId INT
AS
BEGIN
    DELETE FROM Users
    WHERE UserId = @UserId;
END
GO

-- RETRIEVE USERS
CREATE OR ALTER PROCEDURE sp_GetUsers
AS
BEGIN
    SELECT *
    FROM Users
    ORDER BY Surname, FirstName;
END
GO

-- SEARCH USERS BY ID NUMBER, SURNAME, OR ACCOUNT NUMBER
CREATE OR ALTER PROCEDURE sp_SearchUsers
    @SearchTerm NVARCHAR(100)
AS
BEGIN
    SELECT DISTINCT u.*
    FROM Users u
    LEFT JOIN BettingAccounts a ON u.UserId = a.UserId
    WHERE u.IDNumber LIKE '%' + @SearchTerm + '%'
       OR u.Surname LIKE '%' + @SearchTerm + '%'
       OR a.AccountNumber LIKE '%' + @SearchTerm + '%'
    ORDER BY u.Surname, u.FirstName;
END
GO

-- INSERT ACCOUNT
CREATE OR ALTER PROCEDURE sp_InsertAccount
    @AccountNumber NVARCHAR(50),
    @Balance DECIMAL(18,2),
    @IsClosed BIT,
    @CreatedDate DATETIME2,
    @UserId INT
AS
BEGIN
    INSERT INTO BettingAccounts (AccountNumber, Balance, IsClosed, CreatedDate, UserId)
    VALUES (@AccountNumber, @Balance, @IsClosed, @CreatedDate, @UserId);
END
GO

-- UPDATE ACCOUNT
CREATE OR ALTER PROCEDURE sp_UpdateAccount
    @AccountId INT,
    @AccountNumber NVARCHAR(50),
    @IsClosed BIT
AS
BEGIN
    UPDATE BettingAccounts
    SET AccountNumber = @AccountNumber,
        IsClosed = @IsClosed
    WHERE AccountId = @AccountId;
END
GO

-- RETRIEVE ACCOUNTS FOR USER
CREATE OR ALTER PROCEDURE sp_GetAccountsByUserId
    @UserId INT
AS
BEGIN
    SELECT *
    FROM BettingAccounts
    WHERE UserId = @UserId
    ORDER BY CreatedDate DESC;
END
GO

-- INSERT TRANSACTION
CREATE OR ALTER PROCEDURE sp_InsertTransaction
    @AccountId INT,
    @TransactionDate DATETIME2,
    @CaptureDate DATETIME2,
    @Amount DECIMAL(18,2),
    @TransactionType NVARCHAR(20),
    @Description NVARCHAR(255)
AS
BEGIN
    INSERT INTO Transactions
    (
        AccountId,
        TransactionDate,
        CaptureDate,
        Amount,
        TransactionType,
        Description
    )
    VALUES
    (
        @AccountId,
        @TransactionDate,
        @CaptureDate,
        @Amount,
        @TransactionType,
        @Description
    );
END
GO

-- UPDATE TRANSACTION
CREATE OR ALTER PROCEDURE sp_UpdateTransaction
    @TransactionId INT,
    @TransactionDate DATETIME2,
    @CaptureDate DATETIME2,
    @Amount DECIMAL(18,2),
    @TransactionType NVARCHAR(20),
    @Description NVARCHAR(255)
AS
BEGIN
    UPDATE Transactions
    SET TransactionDate = @TransactionDate,
        CaptureDate = @CaptureDate,
        Amount = @Amount,
        TransactionType = @TransactionType,
        Description = @Description
    WHERE TransactionId = @TransactionId;
END
GO

-- RETRIEVE TRANSACTIONS FOR ACCOUNT
CREATE OR ALTER PROCEDURE sp_GetTransactionsByAccountId
    @AccountId INT
AS
BEGIN
    SELECT *
    FROM Transactions
    WHERE AccountId = @AccountId
    ORDER BY TransactionDate DESC;
END
GO