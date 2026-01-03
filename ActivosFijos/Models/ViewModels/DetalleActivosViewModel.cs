

using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace ActivosFijos.Models.ViewModels
{
    public class DetalleActivosViewModel
    {
        public int IdActivos { get; set; }

        public string NumeroInventario { get; set; }
        public string NumeroSerie { get; set; }

        public string Descripcion { get; set; }

        public string Categoria { get; set;  }

        public string Marca { get; set; }

        public string Concepto { get; set; }

        public string Clasificador { get; set;  }

        public string EstadoFisico { get; set; }

        public string EstadoActivo { get; set; }

        public string Almacen { get; set;}

        public List<FotosActivosViewModel> Fotos { get; set; }
    }

    public class FotosActivosViewModel 
    {
        public int IdFoto { get; set; }

        public string RutaFoto { get; set; }
    }


}