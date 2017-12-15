CREATE TABLE baggage_fees
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(1, 1), 
    [Limit] DECIMAL(18, 2) NOT NULL, 
    [FeePerKilogram] MONEY NOT NULL
)
