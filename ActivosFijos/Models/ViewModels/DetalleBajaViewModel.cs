using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActivosFijos.Models.ViewModels
{
    public class DetalleBajaViewModel
    {
        public DetalleActivosViewModel DetalleActivo { get; set; }
        public PLU_OP_BajasActivos PLU_OP_BajasActivos { get; set; }
    }
}