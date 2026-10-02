namespace TicketBooking.Domain;

public class Booking
{
    public Guid Id { get; }
    public Guid EventId { get; }
    public string SeatName { get; }
    public EBookingStatus Status { get; private set; }

    public Booking(Guid eventId, string seatName)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("EventId cannot be empty", nameof(eventId));
        ArgumentException.ThrowIfNullOrWhiteSpace(seatName);

        Id = Guid.NewGuid();
        EventId = eventId;
        SeatName = seatName;
        Status = EBookingStatus.SeatHeld;
    }
    
    
    public void Confirm()
    {
        if (Status != EBookingStatus.SeatHeld)
            throw new InvalidOperationException($"Cannot confirm booking in status {Status}");
        Status = EBookingStatus.Confirmed;
    }
    
    public void Expire()
    {
        if (Status != EBookingStatus.SeatHeld)
            throw new InvalidOperationException($"Cannot expire booking in status {Status}");
        Status = EBookingStatus.Expired;
    }
}