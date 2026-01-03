using System;
using System.ComponentModel.DataAnnotations;


namespace ActivosFijos.Models.ViewModels
{
    public class CambiosActivosViewModel
    {

        public int IdCambioActivo { get; set; }

        public string FolioCambio { get; set; }

        public int NumeroCambios { get; set; }

        public string OficioCambio { get; set; }

        public string NombreReguardante { get; set; }

        public string NombreReguardanteAnterior { get; set; }

        public string UsuarioCambio { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaCreacion { get; set; }
    }
}