using inventario_coprotab.Models.DBInventario;
using inventario_coprotab.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
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

        // POST: Movimiento/CreateModal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModal(MovimientoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var movimiento = new Movimiento
                {
                    IdDispositivo = model.IdDispositivo,
                    TipoMovimiento = model.TipoMovimiento ?? throw new ArgumentNullException(nameof(model.TipoMovimiento)),
                    Cantidad = model.Cantidad,
                    IdUbicacion = model.IdUbicacion,
                    IdResponsable= model.IdResponsable,
                    Observaciones = model.Observaciones,
                    Fecha = model.Fecha,
                };

                _context.Add(movimiento);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }

            ViewData["IdDispositivo"] = new SelectList(_context.Dispositivos.OrderBy(m => m.Nombre), "IdDispositivo", "Nombre");
            ViewData["IdResponsable"] = new SelectList(_context.Responsables.OrderBy(m => m.Nombre), "IdResponsable", "Nombre");
            ViewData["IdUbicacion"] = new SelectList(_context.Ubicaciones.OrderBy(m => m.Nombre), "IdUbicacion", "Nombre");
            ViewBag.TipoDisponibles = new List<string> { "Entrada", "Salida", "Traslado" };



            return PartialView("_CreatePartial", model);


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

        
        
        // GET: Movimiento/Create
        //public IActionResult Create()
        //{
        //    ViewBag.Dispositivos = _context.Dispositivos.Where(d => d.EstadoRegistro).ToList();
        //    ViewBag.Ubicaciones = _context.Ubicaciones.ToList();
        //    ViewBag.Responsables = _context.Responsables.ToList();
        //    return View();
        //}

        //// POST: Movimiento/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(MovimientoViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var dispositivo = await _context.Dispositivos.FindAsync(model.IdDispositivo);
        //        if (dispositivo == null)
        //        {
        //            ModelState.AddModelError("IdDispositivo", "Dispositivo no encontrado");
        //            return View(model);
        //        }

        //        // Validar que el stock no quede negativo en salidas
        //        if (model.TipoMovimiento == "Salida" && dispositivo.StockActual < model.Cantidad)
        //        {
        //            ModelState.AddModelError("Cantidad", $"No hay suficiente stock. Stock actual: {dispositivo.StockActual}");
        //            return View(model);
        //        }

        //        var movimiento = new Movimiento
        //        {
        //            IdDispositivo = model.IdDispositivo,
        //            TipoMovimiento = model.TipoMovimiento ?? "Sin nombre",
        //            Fecha = DateTime.Now,
        //            IdUbicacion = model.IdUbicacion,
        //            IdResponsable = model.IdResponsable,
        //            Cantidad = model.Cantidad,
        //            Observaciones = model.Observaciones
        //        };

        //        _context.Movimientos.Add(movimiento);

        //        // Actualizar stock
        //        if (model.TipoMovimiento == "Entrada")
        //        {
        //            dispositivo.StockActual += model.Cantidad;
        //        }
        //        else if (model.TipoMovimiento == "Salida")
        //        {
        //            dispositivo.StockActual -= model.Cantidad;
        //        }

        //        await _context.SaveChangesAsync();

        //        return RedirectToAction(nameof(Index), "Dispositivo");
        //    }

        //    ViewBag.Dispositivos = _context.Dispositivos.Where(d => d.EstadoRegistro).ToList();
        //    ViewBag.Ubicaciones = _context.Ubicaciones.ToList();
        //    ViewBag.Responsables = _context.Responsables.ToList();
        //    return View(model);
        //}

        //public async Task<IActionResult> Movimientos()
        //{
        //    List<Movimiento> Lista = await _context.Movimientos.Include(d => d.IdUbicacionNavigation)
        //        .Include(d => d.IdDispositivoNavigation)
        //        .Include(d => d.IdResponsableNavigation)
        //        .ToListAsync();
        //    return View(Lista);
        //}

        //// GET: Movimiento/Edit
        //public async Task<IActionResult> Edit(int id)
        //{
        //    var movimiento = await _context.Movimientos.FindAsync(id);
        //    if (movimiento == null)
        //        return NotFound();

        //    var vm = new MovimientoViewModel
        //    {
        //        IdMovimiento = movimiento.IdMovimiento,
        //        IdDispositivo = movimiento.IdDispositivo,
        //        TipoMovimiento = movimiento.TipoMovimiento,
        //        Cantidad = movimiento.Cantidad,
        //        IdUbicacion = movimiento.IdUbicacion,
        //        IdResponsable = movimiento.IdResponsable,
        //        Observaciones = movimiento.Observaciones,
        //        NombreDispositivo = _context.Dispositivos
        //                 .Where(d => d.IdDispositivo == movimiento.IdDispositivo)
        //                 .Select(d => d.Nombre).FirstOrDefault(),
        //        StockActual = _context.Dispositivos
        //                 .Where(d => d.IdDispositivo == movimiento.IdDispositivo)
        //                 .Select(d => (int?)d.StockActual).FirstOrDefault() ?? 0
        //    };


        //    ViewBag.Dispositivos = _context.Dispositivos.Where(d => d.EstadoRegistro).ToList();
        //    ViewBag.Ubicaciones = _context.Ubicaciones.ToList();
        //    ViewBag.Responsables = _context.Responsables.ToList();

        //    return PartialView("_EditMovimiento", vm);

        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, MovimientoViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return PartialView("_EditMovimiento", model);

        //    var movimiento = await _context.Movimientos.FindAsync(id);
        //    if (movimiento == null)
        //        return Json(new { success = false, message = "Movimiento no encontrado" });

        //    movimiento.TipoMovimiento = model.TipoMovimiento ?? "Desconocido";
        //    movimiento.Cantidad = model.Cantidad;
        //    movimiento.IdUbicacion = model.IdUbicacion;
        //    movimiento.IdResponsable = model.IdResponsable;
        //    movimiento.Observaciones = model.Observaciones;

        //    _context.Update(movimiento);
        //    await _context.SaveChangesAsync();

        //    return Json(new
        //    {
        //        success = true,
        //        movimiento = new
        //        {
        //            id = movimiento.IdMovimiento,
        //            tipo = movimiento.TipoMovimiento,
        //            fecha = movimiento.Fecha.ToString("dd/MM/yyyy"),
        //            ubicacion = movimiento.IdUbicacion,
        //            responsable = movimiento.IdResponsable,
        //            cantidad = movimiento.Cantidad,
        //            observaciones = movimiento.Observaciones
        //        }
        //    });
        //}

    }
}