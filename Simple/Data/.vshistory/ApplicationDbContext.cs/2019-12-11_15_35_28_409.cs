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



		public static void GenerateData()
		{
			this.Customers.Add(new Customer() { Name = "Customer_A", IsActive = false });
			this.Customers.Add(new Customer() { Name = "Customer_B", IsActive = true });
			this.Customers.Add(new Customer() { Name = "Customer_C", IsActive = false });

			this.SaveChanges();
		}
	}
}
