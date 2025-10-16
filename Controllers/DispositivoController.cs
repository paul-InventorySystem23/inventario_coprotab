using inventario_coprotab.Models.DBInventario;
using inventario_coprotab.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace inventario_coprotab.Controllers
{
    public class DispositivoController : Controller
    {
        private readonly SistemaInventarioContext _context;

        public DispositivoController(SistemaInventarioContext context)
        {
            _context = context;
        }

        // ✅ Método auxiliar para generar código de inventario automático
        private async Task<string> GenerarCodigoInventario()
        {
            var año = DateTime.Now.Year;

            // Obtener el último código del año actual
            var ultimoCodigo = await _context.Dispositivos
                .Where(d => d.CodigoInventario != null && d.CodigoInventario.StartsWith($"INV-{año}-"))
                .OrderByDescending(d => d.CodigoInventario)
                .Select(d => d.CodigoInventario)
                .FirstOrDefaultAsync();

            int numeroSecuencial = 1;

            if (!string.IsNullOrEmpty(ultimoCodigo))
            {
                // Extraer el número secuencial del último código
                var partes = ultimoCodigo.Split('-');
                if (partes.Length == 3 && int.TryParse(partes[2], out int numero))
                {
                    numeroSecuencial = numero + 1;
                }
            }

            // Formato: INV-2025-0001
            return $"INV-{año}-{numeroSecuencial:D4}";
        }

        // GET: Dispositivo
        // Reemplazar el método Index en DispositivoController.cs

        public async Task<IActionResult> Index(string searchCode, string searchSerie, string searchTipo, string searchEstado)
        {
            // DISPOSITIVOS (Hardware y Consumible)
            var queryDispositivos = _context.Dispositivos
                .Include(d => d.IdMarcaNavigation)
                .Include(d => d.IdTipoNavigation)
                .Where(d => d.EstadoRegistro);

            if (!string.IsNullOrEmpty(searchCode))
                queryDispositivos = queryDispositivos.Where(d => d.CodigoInventario != null && d.CodigoInventario.Contains(searchCode));

            if (!string.IsNullOrEmpty(searchSerie))
                queryDispositivos = queryDispositivos.Where(d => d.NroSerie != null && d.NroSerie.Contains(searchSerie));

            if (!string.IsNullOrEmpty(searchTipo))
                queryDispositivos = queryDispositivos.Where(d => d.IdTipoNavigation != null && d.IdTipoNavigation.Descripcion.Contains(searchTipo));

            if (!string.IsNullOrEmpty(searchEstado))
                queryDispositivos = queryDispositivos.Where(d => d.Estado == searchEstado);

            // ✅ Ordenar por fecha de alta descendente (más recientes primero)
            // Si hay filtros, mostrar todos los resultados; si no, limitar a 5
            List<Dispositivo> dispositivos;
            bool hayFiltros = !string.IsNullOrEmpty(searchCode) ||
                              !string.IsNullOrEmpty(searchSerie) ||
                              !string.IsNullOrEmpty(searchTipo) ||
                              !string.IsNullOrEmpty(searchEstado);

            if (hayFiltros)
            {
                dispositivos = await queryDispositivos
                    .OrderByDescending(d => d.FechaAlta)
                    .ToListAsync();
            }
            else
            {
                dispositivos = await queryDispositivos
                    .OrderByDescending(d => d.FechaAlta)
                    .Take(5)
                    .ToListAsync();
            }

            // COMPONENTES
            var queryComponentes = _context.Componentes
                .Include(c => c.IdMarcaNavigation)
                .Include(c => c.IdTipoNavigation)
                .Where(c => c.EstadoRegistro);

            // Aplicar filtros similares para componentes
            if (!string.IsNullOrEmpty(searchSerie))
                queryComponentes = queryComponentes.Where(c => c.NroSerie != null && c.NroSerie.Contains(searchSerie));

            if (!string.IsNullOrEmpty(searchTipo))
                queryComponentes = queryComponentes.Where(c => c.IdTipoNavigation != null && c.IdTipoNavigation.Descripcion.Contains(searchTipo));

            if (!string.IsNullOrEmpty(searchEstado))
                queryComponentes = queryComponentes.Where(c => c.Estado == searchEstado);

            // ✅ Ordenar por fecha de instalación descendente
            List<Componente> componentes;
            if (hayFiltros)
            {
                componentes = await queryComponentes
                    .OrderByDescending(c => c.FechaInstalacion)
                    .ToListAsync();
            }
            else
            {
                componentes = await queryComponentes
                    .OrderByDescending(c => c.FechaInstalacion)
                    .Take(5)
                    .ToListAsync();
            }

            // MOVIMIENTOS
            var queryMovimientos = _context.Movimientos
                .Include(c => c.IdDispositivoNavigation)
                .Include(c => c.IdUbicacionNavigation)
                .Include(c => c.IdResponsableNavigation);

            // ✅ Ordenar por fecha descendente y tomar solo 5
            var movimientos = await queryMovimientos
                .OrderByDescending(m => m.Fecha)
                .Take(5)
                .ToListAsync();

            // Pasar ambas listas a la vista
            ViewBag.Movimientos = movimientos;
            ViewBag.Componentes = componentes;
            ViewBag.SearchCode = searchCode;
            ViewBag.SearchSerie = searchSerie;
            ViewBag.SearchTipo = searchTipo;
            ViewBag.SearchEstado = searchEstado;

            return View(dispositivos);
        }


        // GET: Dispositivo/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dispositivo = await _context.Dispositivos
                .Include(d => d.IdMarcaNavigation)
                .Include(d => d.IdTipoNavigation)
                .FirstOrDefaultAsync(m => m.IdDispositivo == id);

            if (dispositivo == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsPartial", dispositivo);
        }

        // GET: Dispositivo/Create
        public IActionResult Create()
        {
            ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre");
            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares.OrderBy(t => t.Descripcion), "IdTipo", "Descripcion");
            ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

            var viewModel = new DispositivoCreateViewModel
            {
                IdTipo = 1
            };

            return PartialView("_CreatePartial", viewModel);
        }

        // POST: Dispositivo/CreateModal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModal(DispositivoCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // ✅ Generar código automáticamente
                var codigoInventario = await GenerarCodigoInventario();

                var dispositivo = new Dispositivo
                {
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    IdMarca = model.IdMarca,
                    IdTipo = model.IdTipo,
                    CodigoInventario = codigoInventario, // ✅ Código automático
                    NroSerie = model.NroSerie,
                    Estado = model.Estado ?? "Nuevo",
                    FechaAlta = DateTime.Now, // ✅ DateTime con hora
                    EstadoRegistro = true,
                    StockMinimo = 0
                };

                if (model.IdTipo == 2)
                {
                    dispositivo.StockActual = model.CantidadInicial ?? 0;
                }
                else
                {
                    dispositivo.StockActual = 1;
                }

                _context.Add(dispositivo);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }

            ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre", model.IdMarca);
            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares.OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", model.IdTipo);
            ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

            return PartialView("_CreatePartial", model);
        }
        // GET: Dispositivo/CreateEquipo
        public async Task<IActionResult> CreateEquipo()
        {
            var viewModel = new EquipoCreateViewModel
            {
                Marcas = await _context.Marcas
                    .OrderBy(m => m.Nombre)
                    .Select(m => new SelectListItem { Value = m.IdMarca.ToString(), Text = m.Nombre })
                    .ToListAsync(),

                TiposHardware = await _context.TipoHardwares
                    .Where(t => t.Descripcion == "Hardware") // Solo tipo "Hardware"
                    .Select(t => new SelectListItem { Value = t.IdTipo.ToString(), Text = t.Descripcion })
                    .ToListAsync(),

                ComponentesDisponibles = await _context.Componentes
                    .Where(c => c.EstadoRegistro)
                    .Include(c => c.IdMarcaNavigation) // 👈 ¡Incluye la relación!
                    .Select(c => new ComponenteCheckboxItem
                    {
                        IdComponente = c.IdComponente,
                        NombreCompleto = $"{c.Nombre} - {(c.NroSerie != null ? c.NroSerie : "Sin serie")} ({(c.IdMarcaNavigation != null ? c.IdMarcaNavigation.Nombre : "Sin marca")})"
                    })
                    .ToListAsync(),
            };

            // Forzar IdTipo = Hardware (asumiendo que "Hardware" tiene IdTipo = 1)
            var tipoHardware = await _context.TipoHardwares
                .FirstOrDefaultAsync(t => t.Descripcion == "Hardware");
            if (tipoHardware != null)
                viewModel.IdTipo = tipoHardware.IdTipo;

            return PartialView("_CreateEquipoPartial", viewModel);
        }

        // POST: Dispositivo/CreateEquipoModal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEquipoModal(EquipoCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Recargar listas si hay error
                model.Marcas = await _context.Marcas
                    .OrderBy(m => m.Nombre)
                    .Select(m => new SelectListItem { Value = m.IdMarca.ToString(), Text = m.Nombre })
                    .ToListAsync();

                model.TiposHardware = await _context.TipoHardwares
                    .Where(t => t.Descripcion == "Hardware")
                    .Select(t => new SelectListItem { Value = t.IdTipo.ToString(), Text = t.Descripcion })
                    .ToListAsync();

                model.ComponentesDisponibles = await _context.Componentes
                    .Where(c => c.EstadoRegistro)
                    .Include(c => c.IdMarcaNavigation) // 👈 ¡Incluye!
                    .Select(c => new ComponenteCheckboxItem
                    {
                        IdComponente = c.IdComponente,
                        NombreCompleto = $"{c.Nombre} - {(c.NroSerie != null ? c.NroSerie : "Sin serie")} ({(c.IdMarcaNavigation != null ? c.IdMarcaNavigation.Nombre : "Sin marca")})"
                    })
                    .ToListAsync();

                return PartialView("_CreateEquipoPartial", model);
            }

            // ✅ Crear el dispositivo (equipo)
            var codigoInventario = await GenerarCodigoInventario();

            var equipo = new Dispositivo
            {
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                IdMarca = model.IdMarca,
                IdTipo = model.IdTipo, // Debe ser "Hardware"
                CodigoInventario = codigoInventario,
                NroSerie = model.NroSerie,
                Estado = model.Estado,
                FechaAlta = DateTime.Now,
                EstadoRegistro = true,
                StockActual = 1,
                StockMinimo = 0
            };

            _context.Dispositivos.Add(equipo);
            await _context.SaveChangesAsync(); // Necesario para obtener IdDispositivo

            // ✅ Asociar componentes seleccionados
            foreach (var idComponente in model.ComponentesSeleccionados)
            {
                var relacion = new RelacionDispositivoComponente
                {
                    IdDispositivo = equipo.IdDispositivo,
                    IdComponente = idComponente
                };
                _context.RelacionDispositivoComponentes.Add(relacion);
            }

            await _context.SaveChangesAsync();

            //return Json(new { success = true });
            return RedirectToAction("Index", "Dispositivo");
        }
        // GET: Dispositivo/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dispositivo = await _context.Dispositivos.FindAsync(id);
            if (dispositivo == null)
            {
                return NotFound();
            }

            var Model = new DispositivoEditViewModel
            {
                IdDispositivo = dispositivo.IdDispositivo,
                Nombre = dispositivo.Nombre,
                Descripcion = dispositivo.Descripcion,
                IdMarca = dispositivo.IdMarca,
                IdTipo = dispositivo.IdTipo,
                CodigoInventario = dispositivo.CodigoInventario, // ✅ Solo lectura
                NroSerie = dispositivo.NroSerie,
                Estado = dispositivo.Estado,
                FechaAlta = dispositivo.FechaAlta,
                FechaBaja = dispositivo.FechaBaja,
                StockActual = dispositivo.StockActual,
                StockMinimo = dispositivo.StockMinimo,
                CantidadInicial = dispositivo.StockActual
            };

            var marcas = await _context.Marcas.OrderBy(m => m.Nombre).ToListAsync();
            var tipos = await _context.TipoHardwares.OrderBy(t => t.Descripcion).ToListAsync();

            if (!marcas.Any() || !tipos.Any())
            {
                TempData["ErrorMessage"] = "No hay marcas o tipos disponibles.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["IdMarca"] = new SelectList(marcas, "IdMarca", "Nombre", Model.IdMarca);
            ViewData["IdTipo"] = new SelectList(tipos, "IdTipo", "Descripcion", Model.IdTipo);
            ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

            return PartialView("_EditPartial", Model);
        }

        // POST: Dispositivo/EditModal
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, DispositivoEditViewModel model)
        //{
        //    if (id != model.IdDispositivo)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var dispositivo = await _context.Dispositivos.FindAsync(id);
        //            if (dispositivo == null)
        //            {

        //                dispositivo.Nombre = model.Nombre ?? "Sin nombre";
        //                dispositivo.Descripcion = model.Descripcion;
        //                dispositivo.IdMarca = model.IdMarca ?? 0;
        //                dispositivo.IdTipo = model.IdTipo ?? 0;
        //                // ✅ CodigoInventario NO se modifica (es automático)
        //                dispositivo.NroSerie = model.NroSerie;
        //                dispositivo.Estado = model.Estado;
        //                dispositivo.FechaBaja = model.FechaBaja;
        //                dispositivo.StockMinimo = model.StockMinimo ?? 0;
        //            }
        //            var tipo = await _context.TipoHardwares.FindAsync(model.IdTipo);
        //            if (tipo?.Descripcion?.Trim().ToLower() == "consumible")
        //            {
        //                dispositivo.StockActual = model.CantidadInicial ?? dispositivo.StockActual;
        //            }

        //            _context.Update(dispositivo);
        //            await _context.SaveChangesAsync();

        //            return Json(new { success = true });
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!DispositivoExists(model.IdDispositivo))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //    }

        //    ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre", model.IdMarca);
        //    ViewData["IdTipo"] = new SelectList(_context.TipoHardwares.OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", model.IdTipo);
        //    ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

        //    return PartialView("_EditPartial", model);
        //}

        //// POST: Dispositivo/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var dispositivo = await _context.Dispositivos.FindAsync(id);
        //    if (dispositivo != null)
        //    {
        //        dispositivo.EstadoRegistro = false;
        //        dispositivo.FechaBaja = DateTime.Now; // ✅ Registrar fecha y hora de baja
        //        await _context.SaveChangesAsync();
        //        return Json(new { success = true });
        //    }
        //    return Json(new { success = false });
        //}


        //private bool DispositivoExists(int id)
        //{
        //    return _context.Dispositivos.Any(e => e.IdDispositivo == id);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DispositivoEditViewModel model)
        {
            // Validación temprana del ID
            if (model.IdDispositivo <= 0)
            {
                return NotFound(); // o BadRequest()
            }

            if (!ModelState.IsValid)
            {
                // 🔁 Repetir el mismo código del GET para recargar listas
                var marcas = await _context.Marcas.OrderBy(m => m.Nombre).ToListAsync();
                var tipos = await _context.TipoHardwares.OrderBy(t => t.Descripcion).ToListAsync();

                ViewData["IdMarca"] = new SelectList(marcas, "IdMarca", "Nombre", model.IdMarca);
                ViewData["IdTipo"] = new SelectList(tipos, "IdTipo", "Descripcion", model.IdTipo);
                ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

                return PartialView("_EditPartial", model);
            }

            var dispositivo = await _context.Dispositivos.FindAsync(model.IdDispositivo);
            if (dispositivo == null)
            {
                return NotFound();
            }

            // Actualizar campos (excluyendo CodigoInventario, FechaAlta, etc.)
            dispositivo.Nombre = model.Nombre;
            dispositivo.Descripcion = model.Descripcion;
            dispositivo.IdMarca = model.IdMarca;
            dispositivo.IdTipo = model.IdTipo;
            dispositivo.NroSerie = model.NroSerie;
            dispositivo.Estado = model.Estado;
            dispositivo.FechaBaja = model.FechaBaja;
            dispositivo.StockActual = model.StockActual;
            dispositivo.StockMinimo = model.StockMinimo;

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                // En caso de error, recargar listas y devolver vista con error
                var marcas = await _context.Marcas.OrderBy(m => m.Nombre).ToListAsync();
                var tipos = await _context.TipoHardwares.OrderBy(t => t.Descripcion).ToListAsync();

                ViewData["IdMarca"] = new SelectList(marcas, "IdMarca", "Nombre", model.IdMarca);
                ViewData["IdTipo"] = new SelectList(tipos, "IdTipo", "Descripcion", model.IdTipo);
                ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

                ModelState.AddModelError("", "Ocurrió un error al guardar los cambios.");
                return PartialView("_EditPartial", model);
            }
        }

    }
}