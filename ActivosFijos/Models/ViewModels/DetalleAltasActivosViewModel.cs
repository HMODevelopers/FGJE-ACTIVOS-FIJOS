using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActivosFijos.Models.ViewModels
{
    public class DetalleAltasActivosViewModel
    {
        public string FolioAlta {  get; set; }

        public string NumeroInventario { get; set; }    

        public string Categoria { get; set; }   

        public string Descripcion {  get; set; }    

        public string NumeroSerie { get; set; } 

        public string Marca { get; set; }

        public DateTime FechaAlta { get; set; }
        

    }
}