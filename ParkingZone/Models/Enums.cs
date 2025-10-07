namespace ParkingZone.Models
{
    /// <summary>
    /// Matches SQL: users.role ENUM('client','worker','admin')
    /// </summary>
    public enum UserRole
    {
        client,
        worker,
        admin
    }

    /// <summary>
    /// Matches SQL: spaces.type / vehicles.type ENUM('car','motorcycle','pickup')
    /// </summary>
    public enum VehicleType
    {
        car,
        motorcycle,
        pickup
    }

    /// <summary>
    /// Matches SQL: reservations.status ENUM('active','finished','cancelled')
    /// </summary>
    public enum ReservationStatus
    {
        active,
        finished,
        cancelled
    }

    /// <summary>
    /// Matches SQL: payments.method ENUM('cash','card','transfer')
    /// </summary>
    public enum PaymentMethod
    {
        cash,
        card,
        transfer
    }
}
