using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Citas_.Models
{
    public class UsuarioCitaModel : HomeCitas
    {
            public int IdCita { get; set; }
            public string NombreUsuario { get; set; }
            public string EmailUsuario { get; set; }
            public string MarcaVehiculo { get; set; }
            public string ModeloVehiculo { get; set; }
            public int AnioVehiculo { get; set; }
            public DateTime InitDateCita { get; set; }
            public string EstadoCita { get; set; }
            public string ComentarioCita { get; set; }
            public DateTime? EndDateCita { get; set; }
    
    }
}