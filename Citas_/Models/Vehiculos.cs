using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Citas_.Models
{
    public class Vehiculos
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Marca del Vehiculo")]
        [Required]
        public string Marca { get; set; }

        [DisplayName("Modelo del Vehiculo")]
        [Required]
        public string Modelo { get; set; }

        [DisplayName("Anio del Vehiculo")]
        [Required]
        public int Anio { get; set; }

        [DisplayName("ID del Usuario")]
        [Required]
        public int IdUsuario { get; set; }

        //public virtual Usuarios Usuario { get; set; }
    }
}