using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActivosFijos.Models.ViewModels
{
    public class DetalleBajasActivosViewModel
    {

        public string FolioBajas { get; set; }

        public int? NumeroLote { get; set; } 

        public string NumeroInventario { get; set; }

        public string Categoria { get; set; }

        public string Descripcion { get; set; }

        public string NumeroSerie { get; set; }

        public string Marca { get; set; }

        public DateTime FechaBaja{ get; set; }
    }
}