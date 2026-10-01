namespace TicketBooking.Domain;

public class Booking
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string SeatName { get; private set; }
    public EBookingStatus Status { get; private set; }

    public Booking(Guid eventId, string seatName)
    {
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