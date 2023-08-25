SET NOCOUNT ON
DECLARE @firstCustomer int = 8000001
DECLARE @lastCustomer int = @firstCustomer + 10

DECLARE @today DATETIME = GETDATE()
DECLARE @todayUTC DATETIME = GETUTCDATE()

DECLARE @customerId int = @firstCustomer

BEGIN TRANSACTION AddLoadTestCustomers


    WHILE  @customerId < @lastCustomer
    BEGIN
        PRINT 'Preparing LoadTest customer ' + CAST(@customerId AS varchar);

        -- CustomerService
        -- TODO if needed  CustomerPaymentInfo, CustomerContactInfo, CustomerAddress
        IF NOT EXISTS (SELECT Id FROM CustomerService.dbo.Customer WHERE Id = @customerId)
        BEGIN
            PRINT 'Creating customer record'
            INSERT INTO CustomerService.dbo.Customer (Id,SocialSecurityNumber, Created) VALUES (@customerId, '0000' + CAST(@customerId AS varchar), @today )
        END
        
                -- AccountService
        IF EXISTS (SELECT Id FROM AccountServices.dbo.Accounts WHERE CustomerId = @customerId)
        BEGIN
            PRINT 'Resetting account balance'
            UPDATE AccountServices.dbo.Accounts SET Balance = 100000000 WHERE CustomerId = @customerId
        END
        ELSE
        BEGIN
            PRINT 'Creating account record'

            DECLARE @accountNo varchar(100)
            DECLARE  @accountNoTable table (accountNo varchar(100))

            INSERT  @accountNoTable (accountNo)
            EXEC AccountServices.dbo.GenerateAccountNo
            SET @accountNo = (SELECT accountNo  FROM  @accountNoTable)
            DELETE @accountNoTable

            INSERT INTO AccountServices.dbo.Accounts (AccountID, CustomerID,AccountName,AccountStatusID,AccountTypeID,Reserved,Balance,BalanceTimeUtc ,Created,LastUsed)
            VALUES(@accountNo, @customerId, 'Load Test' + CAST(@customerId AS varchar), 1, 1, 0, 100000000, @todayUTC , @today, @today)
        END


        PRINT 'Creating BetLimitDay, BetLimitMonth and BetLimitSettings records. Deleting any previous records'
        -- BettingService
        IF EXISTS (SELECT Id FROM BettingService.dbo.BetLimitDay WHERE Customer = @customerId)
        BEGIN
            DELETE BettingService.dbo.BetLimitDay WHERE  Customer = @customerId
        END
        INSERT INTO BettingService.dbo.BetLimitDay (Customer, Amount,ValidFrom,AddedAt,IsDefault) VALUES (@customerId, 20000, @today, @today, 0)


        IF  EXISTS (SELECT Id FROM BettingService.dbo.BetLimitMonth WHERE Customer = @customerId)
        BEGIN
            DELETE BettingService.dbo.BetLimitMonth WHERE  Customer = @customerId
        END
        INSERT INTO BettingService.dbo.BetLimitMonth (Customer, Amount,ValidFrom,AddedAt,IsDefault) VALUES (@customerId, 20000, @today, @today, 0)

        IF  EXISTS (SELECT Id FROM BettingService.dbo.BetLimitSetting WHERE Customer = @customerId)
        BEGIN
            DELETE BettingService.dbo.BetLimitSetting WHERE  Customer = @customerId
        END
        INSERT INTO BettingService.dbo.BetLimitSetting (Customer, PlayForPrize,ValidFrom,CustomerCreated,Locked) VALUES (@customerId, -1, @today, @today, 0)

        PRINT 'Deleting BetLimitTransaction(s), BetLimitReservations'

        DELETE BettingService.dbo.BetLimitTransaction  WHERE CustomerId = @customerId
        DELETE BettingService.dbo.BetLimitTransactions WHERE CustomerId = @customerId
        DELETE BettingService.dbo.BetLimitReservation WHERE Customer =  @customerId

        PRINT ''
        SET @customerId = @customerId + 1;
    END -- WHILE
--END -- AddLoadTestCustomers

COMMIT TRANSACTION AddTestCustomer
-- ROLLBACK TRANSACTION AddLoadTestCustomers
