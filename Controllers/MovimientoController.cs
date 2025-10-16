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

        // POST: Movimiento/CreateModal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModal(MovimientoViewModel model)
        {
            if (ModelState.IsValid)
            {
                // ✅ Obtener el dispositivo para actualizar stock
                var dispositivo = await _context.Dispositivos.FindAsync(model.IdDispositivo);
                if (dispositivo == null)
                {
                    ModelState.AddModelError("", "El dispositivo no existe.");
                    return PartialView("_CreatePartial", model);
                }

                // ✅ Validar stock disponible en caso de salida
                if (model.TipoMovimiento == "Salida")
                {
                    if (dispositivo.StockActual < model.Cantidad)
                    {
                        ModelState.AddModelError("Cantidad", $"Stock insuficiente. Disponible: {dispositivo.StockActual}");

                        ViewData["IdResponsable"] = new SelectList(_context.Responsables.OrderBy(m => m.Nombre), "IdResponsable", "Nombre");
                        ViewData["IdUbicacion"] = new SelectList(_context.Ubicaciones.OrderBy(m => m.Nombre), "IdUbicacion", "Nombre");
                        ViewBag.TipoDisponibles = new List<string> { "Entrada", "Salida", "Traslado" };

                        var dispositivoNombre = await _context.Dispositivos.FindAsync(model.IdDispositivo);
                        ViewBag.DispositivoNombre = dispositivoNombre?.Nombre ?? "Desconocido";

                        return PartialView("_CreatePartial", model);
                    }
                }

                var movimiento = new Movimiento
                {
                    IdDispositivo = model.IdDispositivo,
                    TipoMovimiento = model.TipoMovimiento ?? throw new ArgumentNullException(nameof(model.TipoMovimiento)),
                    Cantidad = model.Cantidad,
                    IdUbicacion = model.IdUbicacion,
                    IdResponsable = model.IdResponsable,
                    Observaciones = model.Observaciones,
                    Fecha = DateTime.Now // Siempre usa la fecha y hora actual
                };

                _context.Add(movimiento);

                // ✅ Actualizar stock automáticamente
                if (model.TipoMovimiento == "Entrada")
                {
                    dispositivo.StockActual += model.Cantidad;
                }
                else if (model.TipoMovimiento == "Salida")
                {
                    dispositivo.StockActual -= model.Cantidad;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
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