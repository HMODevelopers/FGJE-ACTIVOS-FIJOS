using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActivosFijos.Models.ViewModels
{
    public class DetalleCambioActivosViewModel
    {
        public string FolioCambio { get; set; }

        public string NumeroInventario { get; set; }

        public string Categoria { get; set; }

        public string Descripcion { get; set; }

        public string NumeroSerie { get; set; }

        public string Marca { get; set; }

        public DateTime FechaCambio { get; set; }
    }
}