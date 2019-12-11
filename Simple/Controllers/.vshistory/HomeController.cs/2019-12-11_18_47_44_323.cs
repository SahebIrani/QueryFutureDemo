using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Simple.Data;
using Simple.Models;
using Simple.SInjulMSBH.Entities;

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


			int count = await Context.Customers.AsNoTracking().CountAsyncEF(cancellationToken);
			int futureCount = Context.Customers.AsNoTracking().Future().Count();
			_logger.LogInformation($"Future Customer Count: {futureCount}");

			QueryFutureValue<Customer> futureFirstCustomer = Context.Customers.AsNoTracking()
				.Where(x => x.IsActive).DeferredFirstOrDefault().FutureValue();
			QueryFutureValue<int> futureCustomerCount = Context.Customers.AsNoTracking()
				.Where(x => x.IsActive).DeferredCount().FutureValue();

			Customer firstCustomer = futureFirstCustomer.Value;
			int countCustomer = futureCustomerCount.Value;
			_logger.LogInformation($"Future Customer Count: {futureCount}");


			QueryFutureValue<int> futureMaxPrice =
				Context.Products.AsNoTracking().DeferredMax(x => x.Prices).FutureValue<int>();

			QueryFutureValue<int> futureMinPrice =
				Context.Products.AsNoTracking().DeferredMin(x => x.Prices).FutureValue<int>();

			int maxPrice = futureMaxPrice.Value;
			int minPrice = futureMinPrice.Value;

			_logger.LogInformation($"Max Price: {maxPrice}");
			_logger.LogInformation($"Min Price: {minPrice}");


			IEnumerable<Product> resultPeopleActive =
				await Context.Products.AsNoTracking()
					.Where(c => c.IsActive)
					.ToListAsyncEF(cancellationToken)
			;
			IEnumerable<Product> resultPeopleNonActive =
				await Context.Products.AsNoTracking()
					.Where(c => !c.IsActive)
					.ToListAsyncEF(cancellationToken)
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


			IQueryable<Product> query = Context.Products.AsNoTracking().Where(c => c.IsActive);

			var pagedQueryable = await query
				.OrderBy(p => p.ProductId)
				.Skip(0).Take(4)
				.Select(p => new
				{
					TotalAsync = query.CountAsyncEF(cancellationToken),
					Item = p
				})
				.ToListAsyncEF(cancellationToken)
			;
			int totalQuery = await pagedQueryable.FirstOrDefault().TotalAsync;
			IEnumerable<Product> items = pagedQueryable.Select(p => p.Item).ToList();

			//var page = await query
			//	.OrderBy(p => p.Name)
			//	.Select(p => p)
			//	.Skip(0).Take(4)
			//	.GroupBy(p => new { TotalAsync = query.CountAsyncEF(cancellationToken) })
			//	.FirstOrDefaultAsyncEF(cancellationToken)
			//;
			//int total = await page.Key.TotalAsync;
			//IEnumerable<Product> people = page.Select(p => p);

			IEnumerable<ProductResult> results = await query
				.OrderBy(p => p.Name)
				.Skip(0).Take(4)
				.Select(p => new ProductResult(p, query.Count()))
				.ToListAsyncEF(cancellationToken)
			;
			int totalCount = results.FirstOrDefault().TotalCount;
			IEnumerable<ProductResult> peopleResult = results.Select(p => p).ToList();

			IQueryable<Product> queryAsQueryable = Context.Products.AsNoTracking().AsQueryable();
			Task<List<Product>> resultsTask =
				query.OrderBy(p => p.ProductId).Skip(0).Take(4).ToListAsyncEF(cancellationToken);
			Task<int> countTask = query.CountAsyncEF(cancellationToken);
			await Task.WhenAll(resultsTask, countTask);
			int TotaslCount = await countTask;
			IEnumerable<Product> Data = await resultsTask;


			IQueryable<Customer> baseQuery = Context.Customers.AsNoTracking().Where(c => c.IsActive);

			var customerResult = await baseQuery
				.Select(e => new { Total = Sql.Ext.Count().Over().ToValue(), Item = e })
				.Skip(0).Take(4)
				.ToLinqToDB()
				.ToListAsyncEF(cancellationToken)
			;
			int totalCustomers = customerResult.First().Total;
			IEnumerable<Customer> customersData = customerResult.Select(c => c.Item).ToList();

			QueryFutureEnumerable<Customer> getTotalCountFuture = baseQuery.Future();
			QueryFutureEnumerable<Customer> getPageFuture = baseQuery.Skip(0).Take(4).Future();
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

	internal class ProductResult
	{
		public Product Product { get; }
		public int TotalCount { get; }

		public ProductResult(Product product, int totalCount)
		{
			Product = product;
			TotalCount = totalCount;
		}

		public override bool Equals(object obj)
		{
			return obj is ProductResult other &&
				   EqualityComparer<Product>.Default.Equals(Product, other.Product) &&
				   TotalCount == other.TotalCount;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Product, TotalCount);
		}
	}
}
