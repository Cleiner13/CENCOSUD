using System;

namespace CencosudBackend.DTOs
{
    public class AdminSerieDiariaItemDto
    {
        public DateTime Fecha { get; set; }   // 2026-01-05
        public int Venta { get; set; }
        public int Coordinado { get; set; }
        public int SinOferta { get; set; }
    }
}
