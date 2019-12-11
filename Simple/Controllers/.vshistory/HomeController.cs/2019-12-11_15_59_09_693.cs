using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Simple.Data;
using Simple.Models;

namespace Simple.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public ApplicationDbContext Context { get; }

		public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
		{
			_logger = logger;
			Context = context;
		}

		public async Task<IActionResult> IndexAsync(CancellationToken cancellationToken = default)
		{
			await Context.Database.EnsureCreatedAsync(cancellationToken);

			await Context.GenerateDataAsync();

			// Oops! The query is already executed, we cannot delay the execution.
			int count = await Context.Customers.CountAsync(cancellationToken);

			// Oops! All customers will be retrieved instead of customer count
			var futureCount = Context.Customers.Future().Count();

			Console.WriteLine("Future Customer Count: {0}", futureCount);

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
