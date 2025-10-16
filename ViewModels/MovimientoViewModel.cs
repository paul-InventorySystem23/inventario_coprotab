using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace inventario_coprotab.ViewModels
{
    public class MovimientoViewModel
    {
        [Required(ErrorMessage = "El dispositivo es obligatorio")]
        public int IdDispositivo { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es obligatorio")]
        [Display(Name = "Tipo de Movimiento")]
        public string? TipoMovimiento { get; set; } // Entrada, Salida, Traslado, Baja

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        public int? IdUbicacion { get; set; }
        public int? IdResponsable { get; set; }
        public string? Observaciones { get; set; }
        public int IdMovimiento { get; set; }

        public DateTime Fecha { get; set; }


        // Para mostrar info del dispositivo
        //public string? NombreDispositivo { get; set; }
        //public string? TipoDispositivo { get; set; }
        //public string? Marca { get; set; }
        //public int StockActual { get; set; }
        //public int StockMinimo { get; set; }

        public List<SelectListItem> Dispositivo { get; set; } = new();
        //public List<SelectListItem> TiposHardware { get; set; } = new();
        public List<ComponenteCheckboxItem> Responsable { get; set; } = new();

        public List<ComponenteCheckboxItem> Ubicacion { get; set; } = new();

    }
}