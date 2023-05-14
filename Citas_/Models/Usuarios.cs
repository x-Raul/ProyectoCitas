using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Citas_.Models
{
    public class Usuarios
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Nombre del usuario")]
        [Required]
        [StringLength(50, ErrorMessage = "El {} debe tener al menos {1} caracter", MinimumLength = 1)]
        public string Nombre { get; set; }

        [DisplayName("Email del usuario")]
        [Required]
        [StringLength(50, ErrorMessage = "El {} debe tener al menos {1} caracter", MinimumLength = 1)]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        [DisplayName("Contrasena del usuario")]
        [Required]
        [StringLength(50, ErrorMessage = "El {} debe tener al menos {1} caracter", MinimumLength = 1)]
        public string Pass { get; set; }

        [DisplayName("Tipo del usuario")]
        [Required]
        public string Tipo { get; set; }


        public string ConfPass { get; set; }

    }
}