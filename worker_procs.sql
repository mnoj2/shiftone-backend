-- ================================================
-- Tables Definition
-- ================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AttendanceRecords')
BEGIN
    CREATE TABLE AttendanceRecords (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Date DATE NOT NULL,
        Status VARCHAR(50) NOT NULL,
        SignInTime DATETIME NULL,
        SignOffTime DATETIME NULL,
        TotalHours FLOAT NULL,
        CONSTRAINT FK_Attendance_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
    );
END
GO

-- Note: Shifts table is removed as per consolidation plan.
GO

-- ================================================
-- Worker Role Stored Procedures
-- ================================================

CREATE OR ALTER PROCEDURE sp_add_attendance (
    @UserId      INT,
    @Date        DATE,
    @Status      VARCHAR(50),
    @SignInTime  DATETIME = NULL,
    @SignOffTime DATETIME = NULL,
    @TotalHours  FLOAT = NULL,
    @status_code VARCHAR(1) OUTPUT,
    @status_msg  VARCHAR(MAX) OUTPUT
) AS
BEGIN
    SET NOCOUNT ON;
    IF @UserId IS NULL OR @UserId <= 0
    BEGIN
        SET @status_code = 'F'; SET @status_msg = 'Valid User ID is required'; RETURN;
    END

    BEGIN TRY
        IF EXISTS (SELECT 1 FROM AttendanceRecords WHERE UserId = @UserId AND Date = @Date)
        BEGIN
            SET @status_code = 'F'; SET @status_msg = 'Record already exists for this date'; RETURN;
        END

        INSERT INTO AttendanceRecords (UserId, Date, Status, SignInTime, SignOffTime, TotalHours)
        VALUES (@UserId, @Date, @Status, @SignInTime, @SignOffTime, @TotalHours);

        SET @status_code = 'S'; SET @status_msg = 'Action successful';
    END TRY
    BEGIN CATCH
        SET @status_code = 'F'; SET @status_msg = 'Database error: ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ================================================

CREATE OR ALTER PROCEDURE sp_update_attendance (
    @Id          INT,
    @Status      VARCHAR(50),
    @SignInTime  DATETIME = NULL,
    @SignOffTime DATETIME = NULL,
    @TotalHours  FLOAT = NULL,
    @status_code VARCHAR(1) OUTPUT,
    @status_msg  VARCHAR(MAX) OUTPUT
) AS
BEGIN
    SET NOCOUNT ON;
    IF @Id IS NULL OR @Id <= 0
    BEGIN
        SET @status_code = 'F'; SET @status_msg = 'Valid ID is required'; RETURN;
    END

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM AttendanceRecords WHERE Id = @Id)
        BEGIN
            SET @status_code = 'F'; SET @status_msg = 'Record not found'; RETURN;
        END

        UPDATE AttendanceRecords
        SET Status = @Status,
            SignInTime = ISNULL(@SignInTime, SignInTime),
            SignOffTime = ISNULL(@SignOffTime, SignOffTime),
            TotalHours = ISNULL(@TotalHours, TotalHours)
        WHERE Id = @Id;

        SET @status_code = 'S'; SET @status_msg = 'Update successful';
    END TRY
    BEGIN CATCH
        SET @status_code = 'F'; SET @status_msg = 'Database error: ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ================================================

CREATE OR ALTER PROCEDURE sp_get_shift_by_worker_date (
    @WorkerId   INT,
    @Date       DATE,
    @status_code VARCHAR(1) OUTPUT,
    @status_msg  VARCHAR(MAX) OUTPUT
) AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM AttendanceRecords WHERE UserId = @WorkerId AND Date = @Date)
        BEGIN
            SET @status_code = 'F'; SET @status_msg = 'No shift or attendance found for today'; RETURN;
        END

        SELECT * FROM AttendanceRecords WHERE UserId = @WorkerId AND Date = @Date;
        SET @status_code = 'S'; SET @status_msg = 'Data fetch successful';
    END TRY
    BEGIN CATCH
        SET @status_code = 'F'; SET @status_msg = 'Database error: ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ================================================
-- TEST EXECUTION EXAMPLES
-- ================================================
/*
DECLARE @status_code VARCHAR(1);
DECLARE @status_msg VARCHAR(MAX);

-- TEST 1: Sign in
EXEC sp_add_attendance @UserId=2, @Date='2026-03-09', @Status='SignedIn', @SignInTime='2026-03-09T08:00:00', @status_code = @status_code OUTPUT, @status_msg = @status_msg OUTPUT;
PRINT @status_code + ' ' + @status_msg;

-- TEST 2: Get Shift
EXEC sp_get_shift_by_worker_date @WorkerId=2, @Date='2026-03-09', @status_code = @status_code OUTPUT, @status_msg = @status_msg OUTPUT;
PRINT @status_code + ' ' + @status_msg;
*/
