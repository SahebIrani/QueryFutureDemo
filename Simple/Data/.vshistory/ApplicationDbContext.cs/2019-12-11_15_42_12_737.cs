using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Simple.SInjulMSBH.Entities;

namespace Simple.Data
{
	public class ApplicationDbContext : IdentityDbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public DbSet<Customer> Customers { get; set; }
		public DbSet<Product> Products { get; set; }



		public async Task GenerateDataAsync()
		{
			IList<Customer> customers = new List<Customer>()
			{
				new Customer{Name = "Sinjul", IsActive = true},
				new Customer{Name = "MSBH", IsActive = true},
				new Customer{Name = "Jack", IsActive = true},
				new Customer{Name = "Slater", IsActive = true},
			};
			await Customers.AddRangeAsync(customers);

			IList<Product> products = new List<Product>()
			{
				new Product{ Name = "Mohsen Chavoshi - NoName (Digital)" , Prices = 20000 , IsActive = true },
				new Product{ Name = "Mohsen Chavoshi - NoName (Physical)" , Prices = 30000 , IsActive = true },
			};
			await Products.AddRangeAsync(products);

			await SaveChangesAsync();
		}
	}
}
