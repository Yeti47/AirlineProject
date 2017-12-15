CREATE TABLE seats
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(1, 1), 
    [FlightId] INT NOT NULL, 
    [PosX] INT NOT NULL, 
    [PosY] INT NOT NULL, 
    [PassengerId] INT NOT NULL,
	CONSTRAINT UNQ_seatnumber UNIQUE(FlightId, PosX, PosY)   
)
