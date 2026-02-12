namespace CencosudBackend.DTOs.AdminReportes
{
    public class AdminHoraWapeoRowDto
    {
        public string Supervisor { get; set; } = "";
        public string Promotor { get; set; } = "";
        public int NroTc { get; set; }

        // clave: "08","09","10"... "22"
        public Dictionary<string, int> Horas { get; set; } = new Dictionary<string, int>();
    }
}
