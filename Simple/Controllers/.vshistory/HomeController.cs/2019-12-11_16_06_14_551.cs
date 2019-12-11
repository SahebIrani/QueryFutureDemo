using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Simple.Data;
using Simple.Models;

using Z.EntityFramework.Plus;

namespace Simple.Controllers
{
	public class HomeController : Controller
	{
		public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
		{
			_logger = logger;
			Context = context;
		}

		private readonly ILogger<HomeController> _logger;
		public ApplicationDbContext Context { get; }

		public async Task<IActionResult> IndexAsync(CancellationToken cancellationToken = default)
		{
			await Context.Database.EnsureCreatedAsync(cancellationToken);

			await Context.GenerateDataAsync(cancellationToken);

			// Oops! The query is already executed, we cannot delay the execution.
			int count = await Context.Customers.CountAsync(cancellationToken);

			// Oops! All customers will be retrieved instead of customer count
			var futureCount = Context.Customers.Future().Count();

			Console.WriteLine("Future Customer Count: {0}", futureCount);

			// GET the minimum and maximum product prices
			var futureMaxPrice = Context.Products.DeferredMax(x => x.Prices).FutureValue<int>();
			var futureMinPrice = Context.Products.DeferredMin(x => x.Prices).FutureValue<int>();

			// TRIGGER all pending queries
			int maxPrice = futureMaxPrice.Value;

			// The future query is already resolved and contains the result
			int minPrice = futureMinPrice.Value;

			int i = 0;

			Console.WriteLine("Max Price: {0}", maxPrice);
			Console.WriteLine("Min Price: {0}", minPrice);


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
