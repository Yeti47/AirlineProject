CREATE TABLE baggage
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(1, 1), 
    [FlightId] INT NULL, 
    [PassengerId] INT NULL, 
    [Weight] DECIMAL(18, 2) NULL, 
    [Fee] MONEY NULL, 
    [State] INT NOT NULL, 
    CONSTRAINT [FK_baggage_flights] FOREIGN KEY ([FlightId]) REFERENCES [flights]([Id]), 
    CONSTRAINT [FK_baggage_passengers] FOREIGN KEY ([PassengerId]) REFERENCES [passengers]([Id])

)
