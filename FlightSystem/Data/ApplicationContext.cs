using FlightSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightSystem.Data
{
    public class ApplicationContext: DbContext
    {
        public DbSet<Flight> Flights { get; set; }
        public DbSet<ClientInfo> Clients { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) 
        {

        }
    }
}
