﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using BookPortal.Web.Services;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.OptionsModel;

namespace BookPortal.Web.Controllers
{
    [Route("api/[controller]")]
    public class ImportersController : Controller
    {
        private readonly ImporterService _importerService;
        private readonly IOptions<AppSettings> _appSettings;

        public ImportersController(ImporterService importerService, IOptions<AppSettings> appSettings)
        {
            _importerService = importerService;
            _appSettings = appSettings;
        }

        [HttpGet("ozon/{bookid}")]
        public async Task<IActionResult> ImportOzon(int bookid)
        {
            using (var client = new HttpClient())
            {
                var requestUrl = new Uri($"{_appSettings.Options.ImportOzonUrl}{bookid}");

                HttpResponseMessage response = await client.GetAsync(requestUrl);
                string html = await response.Content.ReadAsStringAsync();

                var importEdition = _importerService.ParseOzonPage(html);
                return new WrappedObjectResult(importEdition);
            }
        }
    }
}
