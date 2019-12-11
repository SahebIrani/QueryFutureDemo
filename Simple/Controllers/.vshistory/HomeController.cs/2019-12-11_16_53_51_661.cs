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
			_logger.LogInformation($"Future Customer Count: {futureCount}");


			QueryFutureValue<int> futureMaxPrice =
				Context.Products.DeferredMax(x => x.Prices).FutureValue<int>();

			QueryFutureValue<int> futureMinPrice =
				Context.Products.DeferredMin(x => x.Prices).FutureValue<int>();

			int maxPrice = futureMaxPrice.Value;
			int minPrice = futureMinPrice.Value;

			_logger.LogInformation($"Max Price: {maxPrice}");
			_logger.LogInformation($"Min Price: {minPrice}");


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

			QueryFutureEnumerable<Product> resultPeopleActiveWithFuture =
				Context.Products.AsNoTracking()
					.Where(c => c.IsActive)
					.Future()
			;
			QueryFutureEnumerable<Product> resultPeopleNonActiveWithFuture =
				Context.Products.AsNoTracking()
					.Where(c => !c.IsActive)
					.Future()
			;

			IEnumerable<Product> resultPeopleActiveWith =
				await resultPeopleActiveWithFuture.ToListAsync(cancellationToken);

			IEnumerable<Product> resultPeopleNonActiveWith =
				await resultPeopleNonActiveWithFuture.ToListAsync(cancellationToken);

			if (resultPeopleActive == resultPeopleActiveWith
				&& resultPeopleNonActive == resultPeopleNonActiveWith)
			{
				_logger.LogInformation("Results are the same .. !!!!");
			}


			//◘◘◘◘ Paging ◘◘◘◘

			IQueryable<Product> query = Context.Products.Where(c => c.IsActive);
			var page = await query.OrderBy(p => p.Name).Select(p => p.Name).Skip(1).Take(4).GroupBy(p => new { Total = query.Count() }).FirstOrDefaultAsync(cancellationToken);
			int total = page.Key.Total;
			var people = page.Select(p => p);



			IQueryable<Customer> baseQuery = Context.Customers.Where(c => c.IsActive);
			QueryFutureEnumerable<Customer> getTotalCountFuture = baseQuery.Future();
			QueryFutureEnumerable<Customer> getPageFuture = baseQuery.Skip(1).Take(4).Future();
			int getTotalCountResult = getTotalCountFuture.Count();
			IEnumerable<Customer> getPageResult = await getPageFuture.ToListAsync(cancellationToken);


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
