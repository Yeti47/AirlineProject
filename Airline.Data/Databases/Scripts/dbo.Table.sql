CREATE TABLE bookings
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [PassengerId] INT NOT NULL, 
    [FlightId] INT NOT NULL, 
    [IsWaiting] TINYINT NOT NULL, 
    CONSTRAINT [FK_bookings_passenger] FOREIGN KEY ([PassengerId]) REFERENCES [passengers]([Id])
)
