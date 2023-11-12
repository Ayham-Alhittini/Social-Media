using Dating_App_Backend.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Dating_App_Backend.Data
{
    public class DataContext : IdentityDbContext<AppUser, AppRole, int,
        IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>,
        IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions options) : base(options)
        {
            
        }
        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
            builder.Properties<DateOnly>()
                .HaveConversion<DateOnlyConverter>()
                .HaveColumnType("date");
        }
        /// <summary>
        /// Converts <see cref="DateOnly" /> to <see cref="DateTime"/> and vice versa.
        /// </summary>
        public class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
        {
            /// <summary>
            /// Creates a new instance of this converter.
            /// </summary>
            public DateOnlyConverter() : base(
                    d => d.ToDateTime(TimeOnly.MinValue),
                    d => DateOnly.FromDateTime(d))
            { }
        }
        public DbSet<UserLike> Likes { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<Group> Groups { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<ChatGroup> ChatGroups { get; set; }
        public DbSet<ChatGroupParticipant> ChatGroupParticipants { get; set; }
        public DbSet<ChatGroupParticipantRole> ParticipantRoles { get; set; }
        public DbSet<AppFile> Files { get; set; }
        public DbSet<ParticipantGroupInvite> GroupInvites { get; set; }
        public DbSet<ConnectionRecorder> ConnectionRecorders { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<AppUser>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            builder.Entity<AppRole>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();



            builder.Entity<UserLike>()
                .HasKey(l => new { l.SourceUserId, l.TargetUserId });

            builder.Entity<UserLike>()
                .HasOne(l => l.SourceUser)
                .WithMany(l => l.LikedUsers)
                .HasForeignKey(l => l.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserLike>()
                .HasOne(l => l.TargetUser)
                .WithMany(l => l.LikedByUsers)
                .HasForeignKey(l =>l.TargetUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(m => m.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.Recipenet)
                .WithMany(m => m.MessagesRecived)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChatGroupParticipant>()
                .HasKey(x => new { x.ParticipantId, x.ChatGroupId });

            builder.Entity<ChatGroupParticipant>()
                .HasOne(x => x.ChatGroup)
                .WithMany(x => x.GroupParticipants)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ParticipantGroupInvite>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserReciviedInvites)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ParticipantGroupInvite>()
                .HasOne(x => x.InvitedBy)
                .WithMany(x => x.UserSendedInvites)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Message>()
                .HasIndex(x => x.GroupName);

            builder.Entity<ConnectionRecorder>()
                .HasKey(c => new { c.UserName, c.GroupName });
        }
    }
}
