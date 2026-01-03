using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ActivosFijos.Models.ViewModels
{
    public class BajasActivosViewModel
    {
        public int IdBajasActivo { get; set; }

        public string FolioBaja { get; set; }

        public int? NumeroLote { get; set; }    

        public int NumeroBajas { get; set; }

        public string OficioBaja { get; set; }

        public string UsuarioBaja { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaCreacion { get; set; }
    }
}