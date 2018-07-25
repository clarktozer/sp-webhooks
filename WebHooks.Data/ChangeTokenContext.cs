using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using WebHooks.Data.Models;

namespace WebHooks.Data
{
    public class ChangeTokenContext : DbContext
    {
        public DbSet<ChangeToken> ChangeTokens { get; set; }

        public ChangeTokenContext(DbContextOptions<ChangeTokenContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChangeToken>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.ListId).IsRequired();
                entity.Property(e => e.WebId).IsRequired();
                entity.Property(e => e.LastChangeToken).IsRequired();
            });
        }

        public ChangeToken GetLatestBySubscriptionId(string subscriptionId)
        {
            var subscriptionGuid = new Guid(subscriptionId);
            return ChangeTokens.SingleOrDefault(row => row.Id == subscriptionGuid);
        }
    }
}
