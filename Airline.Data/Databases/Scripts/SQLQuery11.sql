SELECT * FROM flights
JOIN airports a ON flights.DepartureAirportId = a.Id
JOIN airports b ON flights.DestinationAirportId = b.Id;