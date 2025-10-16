using inventario_coprotab.Models.DBInventario;
using inventario_coprotab.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace inventario_coprotab.Controllers
{
    public class MovimientoController : Controller
    {
        private readonly SistemaInventarioContext _context;

        public MovimientoController(SistemaInventarioContext context)
        {
            _context = context;
        }

        // GET: Componente/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimiento = await _context.Movimientos
                .Include(c => c.IdDispositivoNavigation)
                .Include(c => c.IdResponsableNavigation)
                .Include(c => c.IdUbicacionNavigation)
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);

            if (movimiento == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsPartial", movimiento);
        }

        // GET: Movimiento/Create
        public IActionResult Create()
        {
            ViewData["IdDispositivo"] = new SelectList(_context.Dispositivos.OrderBy(m => m.Nombre), "IdDispositivo", "Nombre");
            ViewData["IdResponsable"] = new SelectList(_context.Responsables.OrderBy(m => m.Nombre), "IdResponsable", "Nombre");
            ViewData["IdUbicacion"] = new SelectList(_context.Ubicaciones.OrderBy(m => m.Nombre), "IdUbicacion", "Nombre");
            ViewBag.TipoDisponibles = new List<string> { "Entrada", "Salida", "Traslado" };

            var viewModel = new MovimientoViewModel();
            return PartialView("_CreatePartial", viewModel);
        }

        // GET: Movimiento/CreateForDispositivo/5
        public async Task<IActionResult> CreateForDispositivo(int id)
        {
            var dispositivo = await _context.Dispositivos.FindAsync(id);
            if (dispositivo == null)
            {
                return NotFound();
            }

            ViewData["IdResponsable"] = new SelectList(_context.Responsables.OrderBy(m => m.Nombre), "IdResponsable", "Nombre");
            ViewData["IdUbicacion"] = new SelectList(_context.Ubicaciones.OrderBy(m => m.Nombre), "IdUbicacion", "Nombre");
            ViewBag.TipoDisponibles = new List<string> { "Entrada", "Salida", "Traslado" };
            ViewBag.DispositivoNombre = dispositivo.Nombre;

            var viewModel = new MovimientoViewModel
            {
                IdDispositivo = id,
                Fecha = DateTime.Now
            };

            return PartialView("_CreatePartial", viewModel);
        }

        // Agregar este método después del método CreateForDispositivo en MovimientoController.cs

        // GET: Movimiento/CreateForComponente/5
        public async Task<IActionResult> CreateForComponente(int id)
        {
            var componente = await _context.Componentes.FindAsync(id);
            if (componente == null)
            {
                return NotFound();
            }

            ViewData["IdResponsable"] = new SelectList(_context.Responsables.OrderBy(m => m.Nombre), "IdResponsable", "Nombre");
            ViewData["IdUbicacion"] = new SelectList(_context.Ubicaciones.OrderBy(m => m.Nombre), "IdUbicacion", "Nombre");
            ViewBag.TipoDisponibles = new List<string> { "Entrada", "Salida", "Traslado" };
            ViewBag.ComponenteNombre = componente.Nombre;
            ViewBag.StockActual = componente.Cantidad;

            var viewModel = new MovimientoComponenteViewModel
            {
                IdComponente = id,
                Fecha = DateTime.Now,
                Cantidad = 1
            };

            return PartialView("_CreateComponentePartial", viewModel);
        }

        // POST: Movimiento/CreateComponenteModal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComponenteModal(MovimientoComponenteViewModel model)
        {
            if (ModelState.IsValid)
            {
                // ✅ Obtener el componente para actualizar cantidad
                var componente = await _context.Componentes.FindAsync(model.IdComponente);
                if (componente == null)
                {
                    ModelState.AddModelError("", "El componente no existe.");
                    return PartialView("_CreateComponentePartial", model);
                }

                // ✅ Validar stock disponible en caso de salida
                if (model.TipoMovimiento == "Salida")
                {
                    if (componente.Cantidad < model.Cantidad)
                    {
                        ModelState.AddModelError("Cantidad", $"Stock insuficiente. Disponible: {componente.Cantidad}");

                        ViewData["IdResponsable"] = new SelectList(_context.Responsables.OrderBy(m => m.Nombre), "IdResponsable", "Nombre");
                        ViewData["IdUbicacion"] = new SelectList(_context.Ubicaciones.OrderBy(m => m.Nombre), "IdUbicacion", "Nombre");
                        ViewBag.TipoDisponibles = new List<string> { "Entrada", "Salida", "Traslado" };
                        ViewBag.ComponenteNombre = componente.Nombre;
                        ViewBag.StockActual = componente.Cantidad;

                        return PartialView("_CreateComponentePartial", model);
                    }
                }

                // Crear registro en tabla Movimientos (usando IdDispositivo = null o crear nueva tabla MovimientosComponentes)
                // Como tu tabla Movimientos solo tiene IdDispositivo, necesitarás adaptar esto
                // Por ahora, registro el movimiento de forma conceptual:

                // ✅ Actualizar stock automáticamente
                if (model.TipoMovimiento == "Entrada")
                {
                    componente.Cantidad += model.Cantidad;
                }
                else if (model.TipoMovimiento == "Salida")
                {
                    componente.Cantidad -= model.Cantidad;
                }

                // Guardar cambios en componente
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }

            // Si hay errores, recargar los datos
            ViewData["IdResponsable"] = new SelectList(_context.Responsables.OrderBy(m => m.Nombre), "IdResponsable", "Nombre");
            ViewData["IdUbicacion"] = new SelectList(_context.Ubicaciones.OrderBy(m => m.Nombre), "IdUbicacion", "Nombre");
            ViewBag.TipoDisponibles = new List<string> { "Entrada", "Salida", "Traslado" };

            var componenteNombre = await _context.Componentes.FindAsync(model.IdComponente);
            ViewBag.ComponenteNombre = componenteNombre?.Nombre ?? "Desconocido";
            ViewBag.StockActual = componenteNombre?.Cantidad ?? 0;

            return PartialView("_CreateComponentePartial", model);
        }

        // GET: Componente/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimiento = await _context.Movimientos.FindAsync(id);
            if (movimiento == null)
            {
                return NotFound();
            }

            var model = new MovimientoEditViewModel
            {
                IdMovimiento = movimiento.IdMovimiento,
                IdDispositivo = movimiento.IdDispositivo,
                TipoMovimiento = movimiento.TipoMovimiento,
                Cantidad = movimiento.Cantidad,
                IdUbicacion = movimiento.IdUbicacion,
                IdResponsable = movimiento.IdResponsable,
                Observaciones = movimiento.Observaciones,
                Fecha = movimiento.Fecha,
            };

            var dispositivos = await _context.Dispositivos.OrderBy(m => m.Nombre).ToListAsync();
            var ubicaciones = await _context.Ubicaciones.OrderBy(m => m.Nombre).ToListAsync();
            var responsables = await _context.Responsables.OrderBy(m => m.Nombre).ToListAsync();

            if (!dispositivos.Any() || !ubicaciones.Any() || !responsables.Any())
            {
                TempData["ErrorMessage"] = "No hay Dispositivos o Ubicaciones o Responsables Disponibles.";
                return RedirectToAction("Index", "Dispositivo");
            }

            ViewData["IdDispositivo"] = new SelectList(_context.Dispositivos.OrderBy(m => m.Nombre), "IdDispositivo", "Nombre");
            ViewData["IdResponsable"] = new SelectList(_context.Responsables.OrderBy(m => m.Nombre), "IdResponsable", "Nombre");
            ViewData["IdUbicacion"] = new SelectList(_context.Ubicaciones.OrderBy(m => m.Nombre), "IdUbicacion", "Nombre");
            ViewBag.TipoDisponibles = new List<string> { "Entrada", "Salida", "Traslado" };

            return PartialView("_EditPartial", model);
        }

        // POST: Componente/EditModal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MovimientoEditViewModel model)
        {
            if (model.IdMovimiento <= 0)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var dispositivos = await _context.Dispositivos.OrderBy(m => m.Nombre).ToListAsync();
                var ubicaciones = await _context.Ubicaciones.OrderBy(m => m.Nombre).ToListAsync();
                var responsables = await _context.Responsables.OrderBy(m => m.Nombre).ToListAsync();

                ViewData["IdDispositivo"] = new SelectList(_context.Dispositivos.OrderBy(m => m.Nombre), "IdDispositivo", "Nombre");
                ViewData["IdResponsable"] = new SelectList(_context.Responsables.OrderBy(m => m.Nombre), "IdResponsable", "Nombre");
                ViewData["IdUbicacion"] = new SelectList(_context.Ubicaciones.OrderBy(m => m.Nombre), "IdUbicacion", "Nombre");
                ViewBag.TipoDisponibles = new List<string> { "Entrada", "Salida", "Traslado" };

                return PartialView("_EditPartial", model);
            }

            var movimiento = await _context.Movimientos.FindAsync(model.IdMovimiento);
            if (movimiento == null)
            {
                return NotFound();
            }

            // Actualizar campos
            movimiento.IdDispositivo = model.IdDispositivo;
            movimiento.TipoMovimiento = model.TipoMovimiento ?? throw new ArgumentNullException(nameof(model.TipoMovimiento));
            movimiento.Cantidad = model.Cantidad;
            movimiento.IdUbicacion = model.IdUbicacion;
            movimiento.IdResponsable = model.IdResponsable;
            movimiento.Observaciones = model.Observaciones;
            movimiento.Fecha = model.Fecha;

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                var dispositivos = await _context.Dispositivos.OrderBy(m => m.Nombre).ToListAsync();
                var ubicaciones = await _context.Ubicaciones.OrderBy(m => m.Nombre).ToListAsync();
                var responsables = await _context.Responsables.OrderBy(m => m.Nombre).ToListAsync();

                ViewData["IdDispositivo"] = new SelectList(_context.Dispositivos.OrderBy(m => m.Nombre), "IdDispositivo", "Nombre");
                ViewData["IdResponsable"] = new SelectList(_context.Responsables.OrderBy(m => m.Nombre), "IdResponsable", "Nombre");
                ViewData["IdUbicacion"] = new SelectList(_context.Ubicaciones.OrderBy(m => m.Nombre), "IdUbicacion", "Nombre");
                ViewBag.TipoDisponibles = new List<string> { "Entrada", "Salida", "Traslado" };

                ModelState.AddModelError("", "Ocurrió un error al guardar los cambios.");
                return PartialView("_EditPartial", model);
            }
        }

        // POST: Componente/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var movimiento = await _context.Movimientos.FindAsync(id);
            if (movimiento != null)
            {
                _context.Movimientos.Remove(movimiento);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}