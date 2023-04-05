using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PdfConverter.Classes;
using PdfConverter.Models;
using System.Diagnostics;
using System.Text;

namespace PdfConverter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Index(FileUploadModel model)
        {
            var file = model.UploadedFile;

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Please choose a valid file.");
                return View();
            }

            string fileName = Path.GetFileName(file.FileName);

            // Ensure that the uploaded file is a PDF
            if (Path.GetExtension(fileName).ToLower() != ".pdf")
            {
                ModelState.AddModelError("file", "Only PDF files are allowed.");
                return View();
            }

            // Read the text content from the PDF
            string content = await PDFParser.GetText(file, parseImages:model.ParseImages);

            if (string.IsNullOrWhiteSpace(content))
            {
                content = "No text found in file";
            }

            ViewBag.FileContent = content;
            ViewBag.FileName = fileName.Replace(".pdf", ".txt");

            TempData["Message"] = $"PDF '{fileName}' uploaded successfully.";
            return View(nameof(Index));
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