using ChoSV.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChoSV.Data
{
    public class ApplicationDBContext : IdentityDbContext<User>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

        // DbSets for your entities
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserWallPost> UserWallPosts { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserViewHistory> UserViewHistories { get; set; }

        public DbSet<University> Universities { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole { Id = "Admin", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "User", Name = "User", NormalizedName = "USER" }
            };
            modelBuilder.Entity<IdentityRole>().HasData(roles);

            // Configure composite key for Favorite (many-to-many)
            modelBuilder.Entity<Favorite>()
                .HasKey(f => new { f.UserId, f.ProductId });

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Product)
                .WithMany(p => p.Favorites)
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ ADD THIS - Fix Product-User relationship
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Seller)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Product-Category many-to-many relationship
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Categories)
                .WithMany(c => c.Products)
                .UsingEntity(j => j.ToTable("ProductCategories"));

            // Configure Category self-referencing relationship
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure User relationships to avoid cascade delete conflicts
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserWallPost>()
                .HasOne(w => w.Owner)
                .WithMany(u => u.WallPosts)
                .HasForeignKey(w => w.UserWallOwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserWallPost>()
                .HasOne(w => w.Poster)
                .WithMany(u => u.PostsMade)
                .HasForeignKey(w => w.PosterId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Notification relationships
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.FromUser)
                .WithMany()
                .HasForeignKey(n => n.FromUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.UserWallPost)
                .WithMany()
                .HasForeignKey(n => n.UserWallPostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Product)
                .WithMany()
                .HasForeignKey(n => n.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure decimal precision for Price
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<UserViewHistory>()
                .HasKey(v => new { v.UserId, v.ProductId });

            modelBuilder.Entity<UserViewHistory>()
                .HasOne(v => v.Viewer)
                .WithMany(u => u.ViewHistories)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserViewHistory>()
                .HasOne(v => v.Product)
                .WithMany(p => p.UserViewsHistories)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
