using System;

namespace ParkingZone.Models
{
    public class PlateScan
    {
        public int Id { get; set; }

        // Datos OCR
        public string? Plate { get; set; }      // "ABC1234" o "1234ABC"
        public string? Digits { get; set; }     // Solo los 4 dígitos
        public float Confidence { get; set; }   // 0..1

        // Caja detectada (opcional)
        public int? BoxX { get; set; }
        public int? BoxY { get; set; }
        public int? BoxW { get; set; }
        public int? BoxH { get; set; }

        // Foto guardada (en la MISMA tabla, como ruta relativa en wwwroot)
        public string? ImagePath { get; set; }  // ej: /uploads/scans/scan_2025....png

        // Auditoría
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
