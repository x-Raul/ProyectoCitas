using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Citas_.Models
{
    public class HomeCitas
    {
        public int Id { get; set; }
        public int VehiculoId { get; set; }
        public DateTime InitDate { get; set; }
        public string Estado { get; set; }
        public string Comentario { get; set; }
        public DateTime? EndDate { get; set; }

    }
}