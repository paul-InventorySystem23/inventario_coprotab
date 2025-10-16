using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace inventario_coprotab.ViewModels
{
    public class MovimientoComponenteViewModel
    {
        [Required(ErrorMessage = "El componente es obligatorio")]
        public int IdComponente { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es obligatorio")]
        [Display(Name = "Tipo de Movimiento")]
        public string? TipoMovimiento { get; set; } // Entrada, Salida, Traslado

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        public int? IdUbicacion { get; set; }
        public int? IdResponsable { get; set; }
        public string? Observaciones { get; set; }

        public DateTime Fecha { get; set; }

        public List<SelectListItem> Responsables { get; set; } = new();
        public List<SelectListItem> Ubicaciones { get; set; } = new();
    }
}