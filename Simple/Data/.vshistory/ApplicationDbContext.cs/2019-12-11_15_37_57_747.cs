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
			Customers.Add(new Customer() { Name = "Customer_A", IsActive = false });
			Customers.Add(new Customer() { Name = "Customer_B", IsActive = true });
			Customers.Add(new Customer() { Name = "Customer_C", IsActive = false });

			IList<Customer> customers = new List<Customer>()
			{
				new Customer{Name = "Sinjul", IsActive = true},
				new Customer{Name = "MSBH", IsActive = true},
				new Customer{Name = "Jack", IsActive = true},
				new Customer{Name = "Slater", IsActive = true},
			};

			Products.AddRangeAsync()

			SaveChanges();
		}
	}
}
