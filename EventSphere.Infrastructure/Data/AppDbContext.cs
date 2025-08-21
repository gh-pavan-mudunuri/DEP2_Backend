using EventSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class AppDbContext : DbContext

    {
        public DbSet<EventOccurrence> EventOccurrences { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Registration> Registrations { get; set; }
        public DbSet<Payment> Payments { get; set; }

        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<EventSphere.Domain.Entities.Event> Events { get; set; }
        public DbSet<EventSpeaker> EventSpeakers { get; set; }
        public DbSet<EventFaq> EventFaqs { get; set; }
        public DbSet<EventMedia> EventMedias { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<WebsiteReview> WebsiteReviews { get; set; }
        public DbSet<EventFeedback> EventFeedbacks { get; set; }
        // Remove the object EventRegistrations property, use Registrations instead

        protected override void OnModelCreating(ModelBuilder modelBuilder)

        {
            // Ensure identity columns are configured for EF Core
            modelBuilder.Entity<EventFaq>()
                .Property(f => f.FaqId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<EventSpeaker>()
                .Property(s => s.SpeakerId)
                .ValueGeneratedOnAdd();

            // Avoid multiple cascade paths for all relevant entities
            // Bookmark
            modelBuilder.Entity<Bookmark>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookmarks)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>(entity =>
{
    entity.Property(u => u.Role)
          .HasConversion<string>() 
          .HasMaxLength(50); 
});

            modelBuilder.Entity<Bookmark>()
                .HasOne(b => b.Event)
                .WithMany(e => e.Bookmarks)
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Event>()
    .Property(e => e.EditEventCount)
    .HasDefaultValue(3);

            modelBuilder.Entity<Event>()
            .Property(e => e.IsCompleted)
            .HasDefaultValue(0);

            // Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Event)
                .WithMany(e => e.Notifications)
                .HasForeignKey(n => n.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            // Registration
            modelBuilder.Entity<Registration>()
                .HasOne(r => r.User)
                .WithMany(u => u.Registrations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Registration>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            // Removed EventOccurrence and TicketId relationships from Registration

            modelBuilder.Entity<Registration>()
                .HasOne(r => r.Payment)
                .WithMany()
                .HasForeignKey(r => r.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment
            modelBuilder.Entity<Payment>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne<Event>()
                .WithMany()
                .HasForeignKey(p => p.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            // Waitlist
            modelBuilder.Entity<Waitlist>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Waitlist>()
                .HasOne(w => w.Event)
                .WithMany()
                .HasForeignKey(w => w.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            // EventFaq
            modelBuilder.Entity<EventFaq>()
                .HasOne(f => f.Event)
                .WithMany(e => e.Faqs)
                .HasForeignKey(f => f.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // EventFeedback
            modelBuilder.Entity<EventFeedback>()
                .HasOne(fb => fb.Event)
                .WithMany()
                .HasForeignKey(fb => fb.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventFeedback>()
                .HasOne(fb => fb.User)
                .WithMany()
                .HasForeignKey(fb => fb.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventFeedback>()
                .HasOne(fb => fb.Registration)
                .WithMany()
                .HasForeignKey(fb => fb.RegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            // EventInvitation
            modelBuilder.Entity<EventInvitation>()
                .HasOne(ei => ei.Event)
                .WithMany()
                .HasForeignKey(ei => ei.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventInvitation>()
                .HasOne(ei => ei.InvitedUser)
                .WithMany()
                .HasForeignKey(ei => ei.InvitedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // EventLog
            modelBuilder.Entity<EventLog>()
                .HasOne(el => el.User)
                .WithMany()
                .HasForeignKey(el => el.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventLog>()
                .HasOne(el => el.Event)
                .WithMany()
                .HasForeignKey(el => el.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            // EventMedia
            modelBuilder.Entity<EventMedia>()
                .HasOne(em => em.Event)
                .WithMany(e => e.Media)
                .HasForeignKey(em => em.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // EventOccurrence
            modelBuilder.Entity<EventOccurrence>()
                .HasOne(eo => eo.Event)
                .WithMany(e => e.Occurrences)
                .HasForeignKey(eo => eo.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // EventSpeaker
            modelBuilder.Entity<EventSpeaker>()
                .HasOne(es => es.Event)
                .WithMany(e => e.Speakers)
                .HasForeignKey(es => es.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
