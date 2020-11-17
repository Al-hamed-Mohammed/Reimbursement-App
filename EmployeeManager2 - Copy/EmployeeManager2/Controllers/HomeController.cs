using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EmployeeManager2.Models;
using EmployeeManager2.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeManager2.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IHostingEnvironment hostingEnvironment;

        public HomeController(IEmployeeRepository employeeRepository,
                              IHostingEnvironment hostingEnvironment)
        {
            _employeeRepository = employeeRepository;
            this.hostingEnvironment = hostingEnvironment;
        }

        public ViewResult Index()
        {
            var model = _employeeRepository.GetAllEmployee();
            return View(model);
        }

        
        public ViewResult Details(int id)
        {
            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                Employee = _employeeRepository.GetEmployee(id),
                PageTitle = "Employee Details"
            };

            return View(homeDetailsViewModel);
        }

        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        [HttpGet]
        public ViewResult Edit(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);
            EmployeeCreateViewModel employeeEditViewModel = new EmployeeCreateViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                ExistingPhotoPath = employee.PhotoPath,
                ReceiptDate = employee.ReceiptDate,
                ReimburseDate = employee.ReimburseDate,
                ReceiptAmount = employee.ReceiptAmount
            };
            return View(employeeEditViewModel);
        }

        [HttpGet]
        public ActionResult delete(int id)
        {
            if (id > 0 || !string.IsNullOrWhiteSpace(id.ToString()))
            {
                _employeeRepository.Delete(id);
                TempData["Message"] = "Record Deleted Succesfully";
                TempData.Keep();
                return RedirectToAction("Index");
            }
            return View("Index");
        }

        [HttpGet]
        public ActionResult Search()
        {
            string txt = HttpContext.Request.Query["txt"].ToString();
            if (!string.IsNullOrWhiteSpace(txt))
            {
                var model = _employeeRepository.SearchEmployee(txt);
                return View("Index",model);

            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult SearchReceipt()
        {
            string from = HttpContext.Request.Query["from"].ToString();
            string to = HttpContext.Request.Query["to"].ToString();
            if (string.IsNullOrWhiteSpace(to))
            {
                to = DateTime.Today.ToString("yyyy-MM-dd");
            }
                if (!string.IsNullOrWhiteSpace(from))
            {
                var model = _employeeRepository.SearchReceipt(from,to);
                return View("Index", model);

            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Create(EmployeeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;
                if (model.Photos != null && model.Photos.Count > 0)
                {
                    foreach (IFormFile photo in model.Photos)
                    {
                        string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "Images");
                        uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        photo.CopyTo(new FileStream(filePath, FileMode.Create));
                    }
                }
                Employee newEmployee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    PhotoPath = uniqueFileName,
                    ReceiptDate = model.ReceiptDate,
                    ReimburseDate = model.ReimburseDate,
                    ReceiptAmount = model.ReceiptAmount
                };

                _employeeRepository.Add(newEmployee);
                return RedirectToAction("details", new { id = newEmployee.Id });
            }
            return View();
        }
        [HttpPost]
        public IActionResult Edit(EmployeeCreateViewModel model)
        {
            
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;
                if (model.Photos != null && model.Photos.Count > 0)
                {
                    foreach (IFormFile photo in model.Photos)
                    {
                        string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "Images");
                        uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        photo.CopyTo(new FileStream(filePath, FileMode.Create));
                    }
                }
                else
                {
                    uniqueFileName = model.ExistingPhotoPath;
                }
                Employee newEmployee = new Employee
                {
                    Id = model.Id,
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    PhotoPath = uniqueFileName,
                     ReceiptDate = model.ReceiptDate,
                    ReimburseDate = model.ReimburseDate,
                    ReceiptAmount = model.ReceiptAmount

                };

                _employeeRepository.Update(newEmployee);
                return RedirectToAction("details", new { id = newEmployee.Id });
            }
            return View();
        }
        


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
