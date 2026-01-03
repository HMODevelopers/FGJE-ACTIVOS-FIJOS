using System;
using System.ComponentModel.DataAnnotations;


namespace ActivosFijos.Models.ViewModels
{
    public class AltasActivosViewModel
    {

        public int IdAltaActivos { get; set; }
        public string FolioAlta { get; set; }
        public int NumeroAltas { get; set; }
        public string OficioAlta { get; set; }
        public string NombreReguardante { get; set; }   
        public string UsuarioAlta { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaCreacion { get; set; }

    }
}