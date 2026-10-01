using TicketBooking.Domain;

namespace TicketBooking.Tests;

public class UnitTest1
{
    [Fact]
    public void HoldSeat_AlreadyHeld_ShouldThrow()
    {
        var evt = new Event("Concert-01",  new []{ "A1"});
        evt.HoldSeat("A1");
        
        Assert.Throws<InvalidOperationException>(() => evt.HoldSeat("A1"));
    }
    
    [Fact]
    public void HoldSeat_AvailableSeat_ShouldBecomeHeld() 
    {
        var evt = new Event("Concert-01",  new []{ "A1"});
        evt.HoldSeat("A1");

        var seat = evt.Seats.FirstOrDefault(s => s.Name == "A1");
        
        Assert.Equal(ESeatStatus.Held, seat.Status);
    }

    [Fact]
    public void HoldSeat_SeatNotExist_ShouldThrow()
    {
        var evt = new Event("Concert-01",  new []{ "A1"});

        Assert.Throws<KeyNotFoundException>(() => evt.HoldSeat("Z9"));
    }

    [Fact]
    public void NewBooking_ShouldBe_SeatHeld()
    {
        var booking = new Booking(Guid.NewGuid(), "A1");
        Assert.Equal(EBookingStatus.SeatHeld, booking.Status);
    }

    [Fact]
    public void Confirm_FromSeatHeld_ShouldBeConfirmed()
    {
        var booking = new Booking(Guid.NewGuid(), "A1");
        booking.Confirm();
        
        Assert.Equal(EBookingStatus.Confirmed, booking.Status);
    }
    
    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ShouldThrow()
    {
        var booking = new Booking(Guid.NewGuid(), "A1");
        booking.Confirm();
        
        Assert.Throws<InvalidOperationException>(() => booking.Confirm());
    }

    [Fact]
    public void Expire_WhenConfirmed_ShouldThrow()
    {
        var booking = new Booking(Guid.NewGuid(), "A1");
        booking.Confirm();
        
        Assert.Throws<InvalidOperationException>(() => booking.Expire());
    }
}