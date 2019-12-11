using System.Collections.Generic;

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



		public void GenerateData()
		{
			IList<Customer> customers = new List<Customer>()
			{
				new Customer{Name = "Sinjul", IsActive = true},
				new Customer{Name = "MSBH", IsActive = true},
				new Customer{Name = "Jack", IsActive = true},
				new Customer{Name = "Slater", IsActive = true},
			};
			Customers.AddRangeAsync(customers);

			IList<Product> products = new List<Product>()
			{
				new Product{ Name = "Mohsen Chavoshi - BiNam" , Prices = 20000 , IsActive = true },
				new Product{ Name = "Mohsen Chavoshi - NoName" , Prices = 30000 , IsActive = true },
			};
			Products.AddRangeAsync(customers);


			Products.AddRangeAsync();
		}
	}
}
