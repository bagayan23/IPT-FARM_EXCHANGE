using FarmExchange.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmExchange.Data
{
    public class FarmExchangeDbContext : DbContext
    {
        public FarmExchangeDbContext(DbContextOptions<FarmExchangeDbContext> options)
            : base(options)
        {
        }

        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Harvest> Harvests { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ForumThread> ForumThreads { get; set; }
        public DbSet<ForumPost> ForumPosts { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- 1. PROFILES CONFIGURATION ---
            modelBuilder.Entity<Profile>(entity =>
            {
                // Trigger configuration for Profiles
                entity.ToTable("Profiles", tb => tb.HasTrigger("TR_Profiles_UpdatedAt"));

                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.UserType).HasConversion<string>();
            });

            // --- 2. HARVESTS CONFIGURATION (Updated) ---
            modelBuilder.Entity<Harvest>(entity =>
            {
                // *** THIS IS THE LINE YOU ASKED FOR ***
                // It ensures EF Core handles the SQL trigger correctly during updates
                entity.ToTable("Harvests", tb => tb.HasTrigger("TR_Harvests_UpdatedAt"));
                // **************************************

                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Category);

                entity.HasOne(e => e.User)
                    .WithMany(p => p.Harvests)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- 3. MESSAGES CONFIGURATION ---
            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("Messages");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SenderId);
                entity.HasIndex(e => e.RecipientId);

                entity.HasOne(e => e.Sender)
                    .WithMany(p => p.SentMessages)
                    .HasForeignKey(e => e.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Recipient)
                    .WithMany(p => p.ReceivedMessages)
                    .HasForeignKey(e => e.RecipientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Harvest)
                    .WithMany(h => h.Messages)
                    .HasForeignKey(e => e.HarvestId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // --- 4. TRANSACTIONS CONFIGURATION ---
            modelBuilder.Entity<Transaction>(entity =>
            {
                // Trigger configuration for Transactions (Safe fallback)
                entity.ToTable("Transactions", tb => tb.HasTrigger("trg_Transactions"));

                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BuyerId);
                entity.HasIndex(e => e.SellerId);
                entity.HasIndex(e => e.HarvestId);

                entity.HasOne(e => e.Harvest)
                    .WithMany(h => h.Transactions)
                    .HasForeignKey(e => e.HarvestId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Buyer)
                    .WithMany(p => p.BuyerTransactions)
                    .HasForeignKey(e => e.BuyerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Seller)
                    .WithMany(p => p.SellerTransactions)
                    .HasForeignKey(e => e.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- 5. REVIEWS CONFIGURATION ---
            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToTable("Reviews");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BuyerId);
                entity.HasIndex(e => e.SellerId);

                entity.HasOne(e => e.Buyer)
                    .WithMany() // Assuming no nav property back
                    .HasForeignKey(e => e.BuyerId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes of users

                entity.HasOne(e => e.Seller)
                    .WithMany()
                    .HasForeignKey(e => e.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- 6. FORUM CONFIGURATION ---
            modelBuilder.Entity<ForumThread>(entity =>
            {
                entity.ToTable("ForumThreads");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Category);

                entity.HasOne(e => e.Author)
                    .WithMany()
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ForumPost>(entity =>
            {
                entity.ToTable("ForumPosts");
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Thread)
                    .WithMany(t => t.Posts)
                    .HasForeignKey(e => e.ThreadId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete posts if thread is deleted

                entity.HasOne(e => e.Author)
                    .WithMany()
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- 7. USER ADDRESSES CONFIGURATION ---
            modelBuilder.Entity<UserAddress>(entity =>
            {
                entity.ToTable("UserAddresses");
                entity.HasKey(e => e.AddressID);

                entity.HasOne<Profile>()
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(e => e.UserID)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}