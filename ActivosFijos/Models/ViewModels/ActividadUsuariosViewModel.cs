using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActivosFijos.Models.ViewModels
{
    public class ActividadUsuariosViewModel
    {
        public int IdUsuario { get; set; }
        public string Nombres { get; set; }           // viene en MAYÚSCULAS desde el SP
        public string Apellidos { get; set; }         // viene en MAYÚSCULAS desde el SP
        public string NombreCompleto { get; set; }    // viene en MAYÚSCULAS desde el SP

        // ALTAS
        public long ALTAS_NUMERO_ACTIVOS { get; set; }        // COUNT_BIG
        public long ALTAS_FOLIOS_ATENDIDOS { get; set; }      // CAST a BIGINT en SP

        // CAMBIOS
        public long CAMBIOS_NUMERO_ACTIVOS { get; set; }
        public long CAMBIOS_FOLIOS_ATENDIDOS { get; set; }

        // INVENTARIOS
        public long INVENTARIOS_NUMERO_ACTIVOS { get; set; }
        public long INVENTARIOS_FOLIOS_ATENDIDOS { get; set; }

        // BAJAS
        public long BAJAS_NUMERO_ACTIVOS { get; set; }
        public long BAJAS_FOLIOS_ATENDIDOS { get; set; }
    }
}