using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace inventario_coprotab.Models.DBInventario;

public partial class SistemaInventarioContext : DbContext
{
    public SistemaInventarioContext()
    {
    }

    public SistemaInventarioContext(DbContextOptions<SistemaInventarioContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Componente> Componentes { get; set; }

    public virtual DbSet<Dispositivo> Dispositivos { get; set; }

    public virtual DbSet<Marca> Marcas { get; set; }

    public virtual DbSet<Movimiento> Movimientos { get; set; }

    public virtual DbSet<RelacionDispositivoComponente> RelacionDispositivoComponentes { get; set; }

    public virtual DbSet<ReparacionDetalle> ReparacionDetalles { get; set; }

    public virtual DbSet<Reparacione> Reparaciones { get; set; }

    public virtual DbSet<Responsable> Responsables { get; set; }

    public virtual DbSet<TipoHardware> TipoHardwares { get; set; }

    public virtual DbSet<Ubicacione> Ubicaciones { get; set; }

    public virtual DbSet<VwHardwareCompleto> VwHardwareCompletos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=MiConexion");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Componente>(entity =>
        {
            entity.HasKey(e => e.IdComponente).HasName("PK__componen__B5F34A8AFEA2C121");

            entity.ToTable("componentes");

            entity.HasIndex(e => e.NroSerie, "UQ__componen__AD64A161BC78FB82").IsUnique();

            entity.Property(e => e.IdComponente).HasColumnName("id_componente");
            entity.Property(e => e.Cantidad)
                .HasDefaultValue(1)
                .HasColumnName("cantidad");
            entity.Property(e => e.Descripcion)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.EstadoRegistro)
                .HasDefaultValue(true)
                .HasColumnName("estado_registro");
            entity.Property(e => e.FechaInstalacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_instalacion");
            entity.Property(e => e.IdMarca).HasColumnName("id_marca");
            entity.Property(e => e.IdTipo).HasColumnName("id_tipo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.NroSerie)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nro_serie");

            entity.HasOne(d => d.IdMarcaNavigation).WithMany(p => p.Componentes)
                .HasForeignKey(d => d.IdMarca)
                .HasConstraintName("FK_Componentes_Marca");

            entity.HasOne(d => d.IdTipoNavigation).WithMany(p => p.Componentes)
                .HasForeignKey(d => d.IdTipo)
                .HasConstraintName("FK_Componentes_TipoHardware");
        });

        modelBuilder.Entity<Dispositivo>(entity =>
        {
            entity.HasKey(e => e.IdDispositivo).HasName("PK__disposit__FD7B94E579B46084");

            entity.ToTable("dispositivos");

            entity.HasIndex(e => e.CodigoInventario, "UQ__disposit__2C4D9A17F375D82C").IsUnique();

            entity.HasIndex(e => e.NroSerie, "UQ__disposit__AD64A1611432F6A2").IsUnique();

            entity.Property(e => e.IdDispositivo).HasColumnName("id_dispositivo");
            entity.Property(e => e.CodigoInventario)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("codigo_inventario");
            entity.Property(e => e.Descripcion)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.EstadoRegistro)
                .HasDefaultValue(true)
                .HasColumnName("estado_registro");
            entity.Property(e => e.FechaAlta)
                .HasColumnType("datetime")
                .HasColumnName("fecha_alta");
            entity.Property(e => e.FechaBaja)
                .HasColumnType("datetime")
                .HasColumnName("fecha_baja");
            entity.Property(e => e.IdMarca).HasColumnName("id_marca");
            entity.Property(e => e.IdTipo).HasColumnName("id_tipo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.NroSerie)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nro_serie");
            entity.Property(e => e.StockActual)
                .HasDefaultValue(0)
                .HasColumnName("stock_actual");
            entity.Property(e => e.StockMinimo)
                .HasDefaultValue(0)
                .HasColumnName("stock_minimo");

            entity.HasOne(d => d.IdMarcaNavigation).WithMany(p => p.Dispositivos)
                .HasForeignKey(d => d.IdMarca)
                .HasConstraintName("FK_Dispositivos_Marca");

            entity.HasOne(d => d.IdTipoNavigation).WithMany(p => p.Dispositivos)
                .HasForeignKey(d => d.IdTipo)
                .HasConstraintName("FK_Dispositivos_TipoHardware");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(e => e.IdMarca).HasName("PK__marca__7E43E99ED6C98E31");

            entity.ToTable("marca");

            entity.HasIndex(e => e.Nombre, "UQ__marca__72AFBCC63D7C2F41").IsUnique();

            entity.Property(e => e.IdMarca).HasColumnName("id_marca");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Movimiento>(entity =>
        {
            entity.HasKey(e => e.IdMovimiento).HasName("PK__movimien__2A071C247C69698C");

            entity.ToTable("movimientos");

            entity.Property(e => e.IdMovimiento).HasColumnName("id_movimiento");
            entity.Property(e => e.Cantidad)
                .HasDefaultValue(1)
                .HasColumnName("cantidad");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdComponente).HasColumnName("id_componente");
            entity.Property(e => e.IdDispositivo).HasColumnName("id_dispositivo");
            entity.Property(e => e.IdResponsable).HasColumnName("id_responsable");
            entity.Property(e => e.IdUbicacion).HasColumnName("id_ubicacion");
            entity.Property(e => e.Observaciones)
                .IsUnicode(false)
                .HasColumnName("observaciones");
            entity.Property(e => e.TipoMovimiento)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("tipo_movimiento");

            entity.HasOne(d => d.IdComponenteNavigation).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.IdComponente)
                .HasConstraintName("FK_Movimientos_Componentes");

            entity.HasOne(d => d.IdDispositivoNavigation).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.IdDispositivo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Movimientos_Dispositivos");

            entity.HasOne(d => d.IdResponsableNavigation).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.IdResponsable)
                .HasConstraintName("FK_Movimientos_Responsables");

            entity.HasOne(d => d.IdUbicacionNavigation).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.IdUbicacion)
                .HasConstraintName("FK_Movimientos_Ubicaciones");
        });

        modelBuilder.Entity<RelacionDispositivoComponente>(entity =>
        {
            entity.HasKey(e => e.IdRelacion).HasName("PK__relacion__51F3AF4CD9EAD903");

            entity.ToTable("relacion_dispositivo_componente");

            entity.HasIndex(e => new { e.IdDispositivo, e.IdComponente }, "UQ_Relacion_Dispositivo_Componente").IsUnique();

            entity.Property(e => e.IdRelacion).HasColumnName("id_relacion");
            entity.Property(e => e.IdComponente).HasColumnName("id_componente");
            entity.Property(e => e.IdDispositivo).HasColumnName("id_dispositivo");
            entity.Property(e => e.Observaciones)
                .IsUnicode(false)
                .HasColumnName("observaciones");

            entity.HasOne(d => d.IdComponenteNavigation).WithMany(p => p.RelacionDispositivoComponentes)
                .HasForeignKey(d => d.IdComponente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Relacion_Componente");

            entity.HasOne(d => d.IdDispositivoNavigation).WithMany(p => p.RelacionDispositivoComponentes)
                .HasForeignKey(d => d.IdDispositivo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Relacion_Dispositivo");
        });

        modelBuilder.Entity<ReparacionDetalle>(entity =>
        {
            entity.HasKey(e => e.IdReparacionDetalle).HasName("PK__reparaci__CDDF71CC91E4E4A2");

            entity.ToTable("reparacion_detalle");

            entity.Property(e => e.IdReparacionDetalle).HasColumnName("id_reparacion_detalle");
            entity.Property(e => e.Accion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("accion");
            entity.Property(e => e.Cantidad)
                .HasDefaultValue(1)
                .HasColumnName("cantidad");
            entity.Property(e => e.IdComponente).HasColumnName("id_componente");
            entity.Property(e => e.IdDispositivo).HasColumnName("id_dispositivo");
            entity.Property(e => e.IdReparacion).HasColumnName("id_reparacion");
            entity.Property(e => e.Observaciones)
                .IsUnicode(false)
                .HasColumnName("observaciones");
            entity.Property(e => e.TipoElemento)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("tipo_elemento");

            entity.HasOne(d => d.IdComponenteNavigation).WithMany(p => p.ReparacionDetalles)
                .HasForeignKey(d => d.IdComponente)
                .HasConstraintName("FK_ReparacionDetalle_Componente");

            entity.HasOne(d => d.IdDispositivoNavigation).WithMany(p => p.ReparacionDetalles)
                .HasForeignKey(d => d.IdDispositivo)
                .HasConstraintName("FK_ReparacionDetalle_Dispositivo");

            entity.HasOne(d => d.IdReparacionNavigation).WithMany(p => p.ReparacionDetalles)
                .HasForeignKey(d => d.IdReparacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReparacionDetalle_Reparacion");
        });

        modelBuilder.Entity<Reparacione>(entity =>
        {
            entity.HasKey(e => e.IdReparacion).HasName("PK__reparaci__5253371FA810C9EB");

            entity.ToTable("reparaciones");

            entity.Property(e => e.IdReparacion).HasColumnName("id_reparacion");
            entity.Property(e => e.Descripcion)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaFinalizacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_finalizacion");
            entity.Property(e => e.FechaInicio)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_inicio");
            entity.Property(e => e.IdDispositivo).HasColumnName("id_dispositivo");
            entity.Property(e => e.IdResponsable).HasColumnName("id_responsable");
            entity.Property(e => e.Observaciones)
                .IsUnicode(false)
                .HasColumnName("observaciones");
            entity.Property(e => e.TipoReparacion)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("tipo_reparacion");

            entity.HasOne(d => d.IdDispositivoNavigation).WithMany(p => p.Reparaciones)
                .HasForeignKey(d => d.IdDispositivo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reparaciones_Dispositivos");

            entity.HasOne(d => d.IdResponsableNavigation).WithMany(p => p.Reparaciones)
                .HasForeignKey(d => d.IdResponsable)
                .HasConstraintName("FK_Reparaciones_Responsables");
        });

        modelBuilder.Entity<Responsable>(entity =>
        {
            entity.HasKey(e => e.IdResponsable).HasName("PK__responsa__99B1C6CE365A1C74");

            entity.ToTable("responsables");

            entity.Property(e => e.IdResponsable).HasColumnName("id_responsable");
            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellido");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Sector)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("sector");
            entity.Property(e => e.Telefono)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<TipoHardware>(entity =>
        {
            entity.HasKey(e => e.IdTipo).HasName("PK__tipo_har__CF9010890D325CB5");

            entity.ToTable("tipo_hardware");

            entity.HasIndex(e => e.Descripcion, "UQ__tipo_har__298336B6187B5560").IsUnique();

            entity.Property(e => e.IdTipo).HasColumnName("id_tipo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("descripcion");
        });

        modelBuilder.Entity<Ubicacione>(entity =>
        {
            entity.HasKey(e => e.IdUbicacion).HasName("PK__ubicacio__81BAA5911FCE1A1F");

            entity.ToTable("ubicaciones");

            entity.Property(e => e.IdUbicacion).HasColumnName("id_ubicacion");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<VwHardwareCompleto>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_HardwareCompleto");

            entity.Property(e => e.CodigoInventario)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("codigo_inventario");
            entity.Property(e => e.DescripcionHardware)
                .IsUnicode(false)
                .HasColumnName("descripcion_hardware");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.FechaAlta).HasColumnName("fecha_alta");
            entity.Property(e => e.FechaBaja).HasColumnName("fecha_baja");
            entity.Property(e => e.IdHardware).HasColumnName("id_hardware");
            entity.Property(e => e.Marca)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("marca");
            entity.Property(e => e.NombreModelo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre_modelo");
            entity.Property(e => e.NroSerie)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nro_serie");
            entity.Property(e => e.TipoHardware)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo_hardware");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
