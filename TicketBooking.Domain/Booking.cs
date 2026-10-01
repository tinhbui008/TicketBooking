namespace TicketBooking.Domain;

public class Booking
{
    public Guid Id { get; private set; }
    public Guid EventId { get; set; }
    public string SeatName { get; set; }
    public EBookingStatus Status { get; private set; }

    public Booking(Guid eventId, string seatName, EBookingStatus status)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        SeatName = seatName;
        Status = EBookingStatus.SeatHeld;
    }
    
    
    public void Confirm()
    {
        if (Status == EBookingStatus.SeatHeld)
        {
            Status = EBookingStatus.Confirmed;
        }
    }
    
    public void Expire()
    {
        if (Status != EBookingStatus.SeatHeld)
        {
            Status = EBookingStatus.Expired;
        }
    }
}