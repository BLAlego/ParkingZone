using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingZone.Migrations
{
    /// <inheritdoc />
    public partial class SeedSpaces : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- AUTOS 001..010 (inserta solo si no existe el código)
IF NOT EXISTS (SELECT 1 FROM spaces WHERE [code] = '001') INSERT INTO spaces ([code],[type],[available],[level],[hourly_rate]) VALUES ('001','car',1,1,0);
IF NOT EXISTS (SELECT 1 FROM spaces WHERE [code] = '002') INSERT INTO spaces ([code],[type],[available],[level],[hourly_rate]) VALUES ('002','car',1,1,0);
IF NOT EXISTS (SELECT 1 FROM spaces WHERE [code] = '003') INSERT INTO spaces ([code],[type],[available],[level],[hourly_rate]) VALUES ('003','car',1,1,0);
IF NOT EXISTS (SELECT 1 FROM spaces WHERE [code] = '004') INSERT INTO spaces ([code],[type],[available],[level],[hourly_rate]) VALUES ('004','car',1,1,0);
IF NOT EXISTS (SELECT 1 FROM spaces WHERE [code] = '005') INSERT INTO spaces ([code],[type],[available],[level],[hourly_rate]) VALUES ('005','car',1,1,0);
IF NOT EXISTS (SELECT 1 FROM spaces WHERE [code] = '006') INSERT INTO spaces ([code],[type],[available],[level],[hourly_rate]) VALUES ('006','car',1,1,0);
IF NOT EXISTS (SELECT 1 FROM spaces WHERE [code] = '007') INSERT INTO spaces ([code],[type],[available],[level],[hourly_rate]) VALUES ('007','car',1,1,0);
IF NOT EXISTS (SELECT 1 FROM spaces WHERE [code] = '008') INSERT INTO spaces ([code],[type],[available],[level],[hourly_rate]) VALUES ('008','car',1,1,0);
IF NOT EXISTS (SELECT 1 FROM spaces WHERE [code] = '009') INSERT INTO spaces ([code],[type],[available],[level],[hourly_rate]) VALUES ('009','car',1,1,0);
IF NOT EXISTS (SELECT 1 FROM spaces WHERE [code] = '010') INSERT INTO spaces ([code],[type],[available],[level],[hourly_rate]) VALUES ('010','car',1,1,0);

-- MOTOS M001..M005 (tu CHECK exige 'motorcycle')
IF NOT EXISTS (SELECT 1 FROM spaces WHERE [code] = 'M001') INSERT INTO spaces ([code],[type],[available],[level],[hourly_rate]) VALUES ('M001','motorcycle',1,1,0);
IF NOT EXISTS (SELECT 1 FROM spaces WHERE [code] = 'M002') INSERT INTO spaces ([code],[type],[available],[level],[hourly_rate]) VALUES ('M002','motorcycle',1,1,0);
IF NOT EXISTS (SELECT 1 FROM spaces WHERE [code] = 'M003') INSERT INTO spaces ([code],[type],[available],[level],[hourly_rate]) VALUES ('M003','motorcycle',1,1,0);
IF NOT EXISTS (SELECT 1 FROM spaces WHERE [code] = 'M004') INSERT INTO spaces ([code],[type],[available],[level],[hourly_rate]) VALUES ('M004','motorcycle',1,1,0);
IF NOT EXISTS (SELECT 1 FROM spaces WHERE [code] = 'M005') INSERT INTO spaces ([code],[type],[available],[level],[hourly_rate]) VALUES ('M005','motorcycle',1,1,0);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM spaces WHERE [code] IN ('001','002','003','004','005','006','007','008','009','010','M001','M002','M003','M004','M005');
");
        }
    }
}