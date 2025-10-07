using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingZone.Models
{
    [Table("Parking")] // table name is capital P as per your schema
    public class Parking
    {
        // No PK in SQL; EF Core requires a key. We'll define a composite key in DbContext.
        [Required]
        [Column("space_id")]
        public int SpaceId { get; set; }

        [Required]
        [Column("vehicle_id")]
        public int VehicleId { get; set; }

        [Required]
        [Column("entry_time")]
        public DateTime EntryTime { get; set; }

        [Column("exit_time")]
        public DateTime? ExitTime { get; set; }

        [Column("profit", TypeName = "decimal(10,2)")]
        public decimal Profit { get; set; } = 0m;

        // Navigation
        public Space? Space { get; set; }
        public Vehicle? Vehicle { get; set; }
    }
}
