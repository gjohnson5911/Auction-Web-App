using System;
using Microsoft.EntityFrameworkCore;

namespace Auction.Models
{
    public class AuctionContext:DbContext
    {
        public AuctionContext(DbContextOptions<AuctionContext> options):base(options){}

        public DbSet<User> Users {get; set;}
        public DbSet<Sale> Sales {get; set;}
    }
}