using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FTPTransfer.SingleSolution.Models.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace FTPTransfer.SingleSolution.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        private readonly IFileProvider fileProvider;

        public UploadController(IFileProvider fileProvider)
        {
            this.fileProvider = fileProvider;
        }


        public IActionResult Index()
        {
            var model = new FilesViewModel();

            if(User.IsInRole("Administrator"))
            {
                foreach (var item in this.fileProvider.GetDirectoryContents(""))
                {
                    model.Files.Add(
                        new FileDetails { Name = item.Name, Path = item.PhysicalPath });
                }
            }
            else
            {
                foreach (var item in this.fileProvider.GetDirectoryContents(User.Identity.Name))
                {
                    model.Files.Add(
                        new FileDetails { Name = item.Name, Path = item.PhysicalPath });
                }
            }
           
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return Content("files not selected");
            }


            foreach (var file in files)
            {
                var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot", "Uploads", User.Identity.Name,
                        file.GetFilename());

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            return RedirectToAction("Files");
        }

        [HttpPost]
        public async Task<IActionResult> UploadFileViaModel(FileInputModel model)
        {
            if (model == null ||
                model.FileToUpload == null || model.FileToUpload.Length == 0)
            {
                return Content("file not selected");
            }


            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot", "Uploads", User.Identity.Name,
                        model.FileToUpload.GetFilename());

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await model.FileToUpload.CopyToAsync(stream);
            }

            return RedirectToAction("Files");
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Content("file not selected");
            }

            var dirPath = Path.Combine(
                      Directory.GetCurrentDirectory(),
                      "wwwroot", "Uploads", User.Identity.Name);


            var path = Path.Combine(
                        dirPath,
                        file.FileName);

            DirectoryInfo dir = new DirectoryInfo(dirPath);

            if (!dir.Exists)
            {
                Directory.CreateDirectory(dirPath);
            }

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return RedirectToAction("Files");
        }

        [HttpDelete]
        public IActionResult DeleteFile(string fileLocation)
        {
            if (System.IO.File.Exists(fileLocation))
            {
                System.IO.File.Delete(fileLocation);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }

        }



        public async Task<IActionResult> Download(string filename)
        {
            if (filename == null)
            {
                return Content("filename not present");
            }



            var dirPath = Path.Combine(
                           Directory.GetCurrentDirectory(),
                           "wwwroot", "Uploads", User.Identity.Name);

            var filepath = Path.Combine(
                           dirPath, filename);

            DirectoryInfo dir = new DirectoryInfo(dirPath);

            if (!dir.Exists)
            {
                Directory.CreateDirectory(dirPath);
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filepath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(filepath), Path.GetFileName(filepath));
        }


        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats " +
                "officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }
    }
}