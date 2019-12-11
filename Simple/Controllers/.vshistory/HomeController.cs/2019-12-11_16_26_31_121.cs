using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Simple.Data;
using Simple.Models;
using Simple.SInjulMSBH.Entities;

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

			int count = await Context.Customers.CountAsync(cancellationToken);
			int futureCount = Context.Customers.Future().Count();
			Console.WriteLine("Future Customer Count: {0}", futureCount);

			QueryFutureValue<int> futureMaxPrice =
				Context.Products.DeferredMax(x => x.Prices).FutureValue<int>();
			QueryFutureValue<int> futureMinPrice =
				Context.Products.DeferredMin(x => x.Prices).FutureValue<int>();
			int maxPrice = futureMaxPrice.Value;
			int minPrice = futureMinPrice.Value;
			Console.WriteLine("Max Price: {0}", maxPrice);
			Console.WriteLine("Min Price: {0}", minPrice);


			IEnumerable<Product> resultPeopleActive =
				await Context.Products.AsNoTracking()
					.Where(c => c.IsActive)
					.ToListAsync(cancellationToken)
			;
			IEnumerable<Product> resultPeopleNonActive =
				await Context.Products.AsNoTracking()
					.Where(c => !c.IsActive)
					.ToListAsync(cancellationToken)
			;

			IEnumerable<Product> resultPeopleActiveWithFuture =
				await Context.Products.AsNoTracking()
					.Where(c => c.IsActive)
					.Future().ToListAsync(cancellationToken)
			;
			IEnumerable<Product> resultPeopleNonActiveWithFuture =
				await Context.Products.AsNoTracking()
					.Where(c => !c.IsActive)
					.Future().ToListAsync(cancellationToken)
			;


			if (resultPeopleActive == resultPeopleActiveWithFuture
				&& resultPeopleNonActive == resultPeopleNonActiveWithFuture)
			{
				Console.WriteLine("Results are the same .. !!!!");
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
