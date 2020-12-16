PROCEDURE [dbo].[InsertUser]
  @FirstName nchar(50),
  @LastName nchar(50),
  @Gender bit,
  @Age int,
  @Nationality nchar(50),
  @Email nchar(50),
  @PhoneNumber nvarchar(50),
  @Country nvarchar(50),
  @Company nchar(100),
  @Password nchar(100)
AS
  SET NOCOUNT ON;
  DECLARE @NationalityID uniqueidentifier,
      @CountryRC nvarchar(3),
      @CompanyID uniqueidentifier
  SELECT @NationalityID = [NationalityID] FROM [dbo].[Nationality] WHERE Title = @Nationality
  SELECT @CountryRC = [CountryRegionCode] FROM [dbo].[CountryRegion] WHERE Name = @Country
  SELECT @CompanyID = CompanyID FROM [dbo].[Company] WHERE Name = @Company
  IF (SELECT @Email FROM [dbo].[UserInfo] WHERE Email = @Email) IS NOT NULL
    RAISERROR('Email exists', 16, 1);
  ELSE BEGIN
    IF @NationalityID IS NULL
      RAISERROR('Incorrect nationality', 16, 1);
    ELSE BEGIN
      IF @CountryRC IS NULL
        RAISERROR('Incorrect country', 16, 1);
      ELSE BEGIN
        IF @CompanyID IS NULL
          RAISERROR('Incorrect company', 16, 1);
        ELSE BEGIN
          BEGIN TRY
            BEGIN TRANSACTION
              INSERT INTO [dbo].[UserInfo] VALUES
              (NEWID(), @FirstName, @LastName, @Gender, @Age, @NationalityID, @Email, @PhoneNumber, @CountryRC, @CompanyID, @Password, GETDATE())
            COMMIT
          END TRY
          BEGIN CATCH
            IF @@TRANCOUNT > 0
            ROLLBACK
            DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT;
            SELECT @ErrorMessage = ERROR_MESSAGE(),@ErrorSeverity = ERROR_SEVERITY();
            RAISERROR(@ErrorMessage, @ErrorSeverity, 1);
          END CATCH;
          END
      END
    END
  END
      SELECT SCOPE_IDENTITY()
     
---------------------------------------------------

PROCEDURE [dbo].[GetLastCommentInfo]
@datetime datetime
AS
  SELECT FirstName, LastName, Gender, Age, Title, PhoneNumber, cr.Name, c.Name, com.Date, com.Text FROM [dbo].[UserInfo] 
  INNER JOIN [dbo].[CountryRegion] cr ON [cr].[CountryRegionCode] = [UserInfo].CountryRegionCode
  INNER JOIN [dbo].[Nationality] n ON [n].[NationalityID] = [UserInfo].[NationalityID]
  INNER JOIN [dbo].[Company] c ON [c].[CompanyID] = [UserInfo].[CompanyID]
  INNER JOIN [dbo].Comment com ON [com].[UserId] = [UserInfo].[UserID] WHERE 
  [com].[Date] > @datetime ORDER BY Date
  
----------------------------------------------------

PROCEDURE [dbo].[MakeComment]
	@Email nchar(50),
	@Password nchar(100),
	@Text nchar(4000)
AS
	SET NOCOUNT ON;
	BEGIN TRY
		BEGIN TRANSACTION
			INSERT INTO [dbo].[Comment] VALUES ((SELECT UserID FROM [dbo].[UserInfo] WHERE Email = @Email AND Password = @Password), @Text, GETDATE())
		COMMIT
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
		ROLLBACK
		DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT;
		SELECT @ErrorMessage = ERROR_MESSAGE(),@ErrorSeverity = ERROR_SEVERITY();
		RAISERROR(@ErrorMessage, @ErrorSeverity, 1);
	END CATCH;
    SELECT SCOPE_IDENTITY()
    
  --------------------------------------------------
  
PROCEDURE [dbo].[GetUserInfo]
	@Email nchar(50),
	@Password nchar(100)
AS
	SELECT FirstName, LastName, Gender, Age, Title, PhoneNumber, cr.Name, c.Name FROM [dbo].[UserInfo] 
  INNER JOIN [dbo].[CountryRegion] cr ON [cr].[CountryRegionCode] = [UserInfo].CountryRegionCode
  INNER JOIN [dbo].[Nationality] n ON [n].[NationalityID] = [UserInfo].[NationalityID]
  INNER JOIN [dbo].[Company] c ON [c].[CompanyID] = [UserInfo].[CompanyID] WHERE Email = @Email AND Password = @Password
