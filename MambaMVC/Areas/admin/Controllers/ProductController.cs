using MambaMVC.DAL;
using Microsoft.AspNetCore.Mvc;

namespace MambaMVC.Areas.admin.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public EmployeeController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            var EmployeeVM = await _context.Employees
                .Include(e => e.Department)
                .Where(e => !e.IsDeleted)
                .Select(e => new GetEmployeeVM
                {
                    Id = e.Id,
                    Name = e.Name,
                    Surname = e.Surname,
                    DepartmentName = e.Department.Name,
                    Image = e.Image,
                    FbLink = e.FbLink,
                    InstagramLink = e.InstagramLink,
                    TwitterLink = e.TwitterLink
                }).ToListAsync();
            return View(EmployeeVM);
        }

        public async Task<IActionResult> Create()
        {
            CreateEmployeeVM employeeVM = new CreateEmployeeVM
            {
                Departments = await _context.Departments.ToListAsync()
            };
            return View(employeeVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmployeeVM employeeVM)
        {
            employeeVM.Departments = await _context.Departments.ToListAsync();
            if (!ModelState.IsValid) return View(employeeVM);

            if (!employeeVM.Image.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(Image), "Image type is incorrect");
                return View(employeeVM);
            }

            if (!employeeVM.Image.ValidateSize(FileSize.MB, 2))
            {
                ModelState.AddModelError("Image", "Image must be less than 2 MB");
                return View(employeeVM);
            }

            bool result = employeeVM.Departments.Any(e => e.Id == employeeVM.DepartmentId);
            if (!result)
            {
                ModelState.AddModelError(nameof(CreateEmployeeVM.DepartmentId), "Department does not exists");
                return View(employeeVM);
            }
            string imagePath = await employeeVM.Image.CreateFileAsync(_env.WebRootPath, "assets", "img");

            Employee employee = new()
            {
                Name = employeeVM.Name,
                Surname = employeeVM.Surname,
                DepartmentId = employeeVM.DepartmentId.Value,
                Image = imagePath,
                FbLink = employeeVM.FbLink,
                InstagramLink = employeeVM.InstagramLink,
                TwitterLink = employeeVM.TwitterLink
            };
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");

        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1) return BadRequest();

            Employee employee = await _context.Employees.FirstOrDefaultAsync(p => p.Id == id);
            if (employee == null) return NotFound();
            UpdateEmployeeVM employeeVM = new()
            {
                Name = employee.Name,
                Surname = employee.Surname,
                DepartmentId = employee.DepartmentId,
                ExistingImage = employee.Image,
                Departments = await _context.Departments.ToListAsync(),
                FbLink = employee.FbLink,
                InstagramLink = employee.InstagramLink,
                TwitterLink = employee.TwitterLink
            };
            return View(employeeVM);

        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateEmployeeVM employeeVM)
        {
            if (id == null || id < 1) return BadRequest();

            Employee existed = await _context.Employees.FirstOrDefaultAsync(p => p.Id == id);
            if (existed == null) return NotFound();

            employeeVM.Departments = await _context.Departments.ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(employeeVM);
            }
            if (employeeVM.Image is not null)
            {
                if (!employeeVM.Image.ValidateType("image/"))
                {
                    ModelState.AddModelError("Image", "Image type is not correct");
                    return View(employeeVM);
                }
                if (!employeeVM.Image.ValidateSize(FileSize.MB, 2))
                {
                    ModelState.AddModelError("Image", "Size of image must be less than 2 mb");
                    return View(employeeVM);
                }

                existed.Image.DeleteFile(_env.WebRootPath, "assets", "img");
                existed.Image = await employeeVM.Image.CreateFileAsync(_env.WebRootPath, "assets", "img");
            }
            else
            {
                existed.Image = employeeVM.ExistingImage;
            }

            if (existed.DepartmentId != employeeVM.DepartmentId)
            {
                bool result = employeeVM.Departments.Any(d => d.Id == employeeVM.DepartmentId);
                if (!result)
                {
                    return View(employeeVM);
                }
            }

            existed.Name = employeeVM.Name;
            existed.Surname = employeeVM.Surname;
            existed.DepartmentId = employeeVM.DepartmentId.Value;
            existed.FbLink = employeeVM.FbLink;
            existed.InstagramLink = employeeVM.InstagramLink;
            existed.TwitterLink = employeeVM.TwitterLink;


            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id < 1) return BadRequest();

            var employee = await _context.Employees.FirstOrDefaultAsync(p => p.Id == id);
            if (employee == null) return NotFound();

            if (!string.IsNullOrEmpty(employee.Image))
            {
                employee.Image.DeleteFile(_env.WebRootPath, "assets", "img");
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");

        }
    }
}
