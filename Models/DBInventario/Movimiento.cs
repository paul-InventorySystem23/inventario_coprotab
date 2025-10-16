using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBInventario;

public partial class Movimiento
{
    public int IdMovimiento { get; set; }

    public int IdDispositivo { get; set; }

    public string TipoMovimiento { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public int? IdUbicacion { get; set; }

    public int? IdResponsable { get; set; }

    public int Cantidad { get; set; }

    public string? Observaciones { get; set; }

    public int? IdComponente { get; set; }

    public virtual Componente? IdComponenteNavigation { get; set; }

    public virtual Dispositivo IdDispositivoNavigation { get; set; } = null!;

    public virtual Responsable? IdResponsableNavigation { get; set; }

    public virtual Ubicacione? IdUbicacionNavigation { get; set; }
}
