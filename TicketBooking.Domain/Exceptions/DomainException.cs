namespace TicketBooking.Domain.Exceptions;

public abstract class DomainException(string message) : Exception(message);

public class EventNotFoundException(Guid eventId) : DomainException($"Event {eventId} does not exist");
public class SeatNotFoundException(string seatName) : DomainException($"Seat with name {seatName} does not exist");
public class SeatNotAvailableException(string seatName, ESeatStatus status) : DomainException($"Seat with name {seatName} is not available (current status: {status})");