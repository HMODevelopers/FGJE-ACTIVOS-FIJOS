using System;
using System.ComponentModel.DataAnnotations;

namespace ActivosFijos.Models.ViewModels
{
    public class InventariosViewModel
    {
        public int? NumeroEmpleado { get; set;  }
        public string FolioInventario { get; set; }
        public string NombreCompleto { get; set; }

        public string Adscripcion { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaInventario { get; set; }

        public int TotalActivos { get; set; }

        public int Encontrados { get; set; }

        public int Pendientes { get; set; }

    }
}