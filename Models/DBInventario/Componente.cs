using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBInventario;

public partial class Componente
{
    public int IdComponente { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int? IdMarca { get; set; }

    public int? IdTipo { get; set; }

    public string? NroSerie { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaInstalacion { get; set; }

    public bool EstadoRegistro { get; set; }

    public int Cantidad { get; set; }

    public virtual Marca? IdMarcaNavigation { get; set; }

    public virtual TipoHardware? IdTipoNavigation { get; set; }

    public virtual ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();

    public virtual ICollection<RelacionDispositivoComponente> RelacionDispositivoComponentes { get; set; } = new List<RelacionDispositivoComponente>();

    public virtual ICollection<ReparacionDetalle> ReparacionDetalles { get; set; } = new List<ReparacionDetalle>();
}
