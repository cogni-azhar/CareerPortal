﻿using CareerPortal.DataAccess.Repository.IRepository;
using CareerPortal.Models;
using CareerPortal.Services.IService;
using CareerPortal.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CareerPortal.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IS3Service _service;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork,IS3Service service)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _service = service;
        }

        public IActionResult Index()
        {
            if (UserInfo == null || !UserInfo.isLoggedIn)
            {
                return View();
            }
            return RedirectToAction(nameof(Index), "Profile");
        }

        public IActionResult Login()
        {
            Login login = new Login();
            return View(login);
        }

        public IActionResult Register()
        {
            User user = new User();
            return View(user);
        }

        public IActionResult Logout()
        {
            UserInfo = new SessionInfo();
            HttpContext.Session.SetString("SessionUser", JsonConvert.SerializeObject(UserInfo));
            return RedirectToAction(nameof(Index));
 
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
        [HttpGet]
        public IActionResult AddFile(string type="Image")
        {
            if (!UserInfo.isLoggedIn) return RedirectToAction(nameof(Login));
            FileUploadObject file = new FileUploadObject();
            file.Type = type;
            return View(file);
        }

        [HttpPost]
        public async Task<IActionResult> AddFile(FileUploadObject test)
        {
            if (!UserInfo.isLoggedIn) return RedirectToAction(nameof(Login));
            if (ModelState.IsValid)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await test.URL.CopyToAsync(memoryStream);
                    if (memoryStream.Length < 2097152)
                    {
                        var BucketName = test.Type.Equals("Image") ? SD.BucketName + @"/" + SD.ProfileImageFolder : SD.BucketName + @"/" + SD.ResumeFolder;
                        var response = await _service.UploadFileAsync(memoryStream, BucketName, UserInfo.UserID.ToString());
                        if (response.UploadSuccessful) return RedirectToAction(nameof(Index), "Profile");
                        else return Ok(response);
                    }

                    ModelState.AddModelError("url", "File must be less than 2 mb");
                }
            }
            return View(test);
        }

        #region

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(Login login)
        {
            if (ModelState.IsValid)
            {
                var UserFromDb = _unitOfWork.User.GetFirstOrDefault(i => i.Email == login.Email);
                if(UserFromDb == null)
                {
                    ModelState.AddModelError("email", "Username incorrect");
                    return View(login);
                }
                if (UserFromDb.Password.Equals(login.Password))
                {
                    UserInfo = new SessionInfo()
                    {
                        isLoggedIn = true,
                        UserID = UserFromDb.Id
                    };
                    HttpContext.Session.SetString("SessionUser", JsonConvert.SerializeObject(UserInfo));
                    return RedirectToAction(nameof(Index), "Profile");
                }
            }
            ModelState.AddModelError("password", "Password incorrect");
            return View(login);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                var ObjFromDb = _unitOfWork.User.GetFirstOrDefault(i => i.Email == user.Email);
                if (ObjFromDb == null)
                {
                    _unitOfWork.User.Add(user);
                    _unitOfWork.Save();

                    UserInfo = new SessionInfo()
                    {
                        isLoggedIn = false,
                        UserID = _unitOfWork.User.GetFirstOrDefault(i => i.Email == user.Email).Id
                    };
                    HttpContext.Session.SetString("SessionUser", JsonConvert.SerializeObject(UserInfo));


                    return RedirectToAction(nameof(Index), "Education");
                }
                ModelState.AddModelError("email", "Email ID already exists"); 
            }   
            return View(user);
        }
        #endregion
    }
}
