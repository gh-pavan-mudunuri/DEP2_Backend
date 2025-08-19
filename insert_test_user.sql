CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [IsEmailVerified] bit NOT NULL,
    [EmailVerificationToken] nvarchar(max) NULL,
    [EmailVerificationTokenExpiry] datetime2 NULL,
    [PasswordResetToken] nvarchar(max) NULL,
    [PasswordResetTokenExpiry] datetime2 NULL,
    [Phone] nvarchar(max) NULL,
    [Role] int NOT NULL,
    [ProfileImage] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
);
GO


CREATE TABLE [Events] (
    [EventId] int NOT NULL IDENTITY,
    [OrganizerId] int NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [CoverImage] nvarchar(max) NOT NULL,
    [VibeVideoUrl] nvarchar(max) NULL,
    [Category] nvarchar(max) NOT NULL,
    [EventType] int NOT NULL,
    [Location] nvarchar(max) NULL,
    [RegistrationDeadline] datetime2 NOT NULL,
    [EventStart] datetime2 NOT NULL,
    [EventEnd] datetime2 NOT NULL,
    [RecurrenceType] int NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [IsPaidEvent] bit NOT NULL,
    [Price] decimal(18,2) NULL,
    [IsVerifiedByAdmin] bit NOT NULL,
    [AdminVerifiedAt] datetime2 NULL,
    [Status] int NOT NULL,
    [PlatformFeePaid] bit NOT NULL,
    [MaxAttendees] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [OrganizerEmail] nvarchar(max) NULL,
    [AdminComments] nvarchar(max) NULL,
    CONSTRAINT [PK_Events] PRIMARY KEY ([EventId]),
    CONSTRAINT [FK_Events_Users_OrganizerId] FOREIGN KEY ([OrganizerId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Bookmark] (
    [BookmarkId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [EventId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UserEmail] nvarchar(max) NULL,
    [EventTitle] nvarchar(max) NULL,
    CONSTRAINT [PK_Bookmark] PRIMARY KEY ([BookmarkId]),
    CONSTRAINT [FK_Bookmark_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Bookmark_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [EventFaq] (
    [FaqId] int NOT NULL IDENTITY,
    [EventId] int NOT NULL,
    [Question] nvarchar(max) NOT NULL,
    [Answer] nvarchar(max) NOT NULL,
    [EventTitle] nvarchar(max) NULL,
    CONSTRAINT [PK_EventFaq] PRIMARY KEY ([FaqId]),
    CONSTRAINT [FK_EventFaq_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [EventInvitation] (
    [InvitationId] int NOT NULL IDENTITY,
    [EventId] int NOT NULL,
    [InvitedUserId] int NOT NULL,
    [InvitedEmail] nvarchar(max) NOT NULL,
    [Status] int NOT NULL,
    [SentAt] datetime2 NOT NULL,
    [RespondedAt] datetime2 NULL,
    [ResponseMessage] nvarchar(max) NULL,
    [EventTitle] nvarchar(max) NULL,
    [OrganizerEmail] nvarchar(max) NULL,
    CONSTRAINT [PK_EventInvitation] PRIMARY KEY ([InvitationId]),
    CONSTRAINT [FK_EventInvitation_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EventInvitation_Users_InvitedUserId] FOREIGN KEY ([InvitedUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [EventLog] (
    [LogId] int NOT NULL IDENTITY,
    [UserId] int NULL,
    [EventId] int NULL,
    [Action] nvarchar(max) NOT NULL,
    [Details] nvarchar(max) NULL,
    [Timestamp] datetime2 NOT NULL,
    [UserEmail] nvarchar(max) NULL,
    [EventTitle] nvarchar(max) NULL,
    CONSTRAINT [PK_EventLog] PRIMARY KEY ([LogId]),
    CONSTRAINT [FK_EventLog_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EventLog_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [EventMedia] (
    [MediaId] int NOT NULL IDENTITY,
    [EventId] int NOT NULL,
    [MediaUrl] nvarchar(max) NOT NULL,
    [MediaType] int NOT NULL,
    [Description] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [EventTitle] nvarchar(max) NULL,
    CONSTRAINT [PK_EventMedia] PRIMARY KEY ([MediaId]),
    CONSTRAINT [FK_EventMedia_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [EventOccurrence] (
    [OccurrenceId] int NOT NULL IDENTITY,
    [EventId] int NOT NULL,
    [StartTime] datetime2 NOT NULL,
    [EndTime] datetime2 NOT NULL,
    [EventTitle] nvarchar(max) NULL,
    CONSTRAINT [PK_EventOccurrence] PRIMARY KEY ([OccurrenceId]),
    CONSTRAINT [FK_EventOccurrence_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [EventSpeaker] (
    [SpeakerId] int NOT NULL IDENTITY,
    [EventId] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [PhotoUrl] nvarchar(max) NULL,
    [Bio] nvarchar(max) NULL,
    [SocialLinks] nvarchar(max) NULL,
    [EventTitle] nvarchar(max) NULL,
    CONSTRAINT [PK_EventSpeaker] PRIMARY KEY ([SpeakerId]),
    CONSTRAINT [FK_EventSpeaker_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Notification] (
    [NotificationId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [EventId] int NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [Type] int NOT NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UserEmail] nvarchar(max) NULL,
    [EventTitle] nvarchar(max) NULL,
    [ActionUrl] nvarchar(max) NULL,
    CONSTRAINT [PK_Notification] PRIMARY KEY ([NotificationId]),
    CONSTRAINT [FK_Notification_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Notification_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Payment] (
    [PaymentId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [EventId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [PaymentMethod] nvarchar(max) NOT NULL,
    [TransactionId] nvarchar(max) NOT NULL,
    [Status] int NOT NULL,
    [PaymentTime] datetime2 NOT NULL,
    [UserEmail] nvarchar(max) NULL,
    [EventTitle] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    [UserId1] int NULL,
    [EventId1] int NULL,
    CONSTRAINT [PK_Payment] PRIMARY KEY ([PaymentId]),
    CONSTRAINT [FK_Payment_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Payment_Events_EventId1] FOREIGN KEY ([EventId1]) REFERENCES [Events] ([EventId]),
    CONSTRAINT [FK_Payment_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Payment_Users_UserId1] FOREIGN KEY ([UserId1]) REFERENCES [Users] ([UserId])
);
GO


CREATE TABLE [Ticket] (
    [TicketId] int NOT NULL IDENTITY,
    [EventId] int NOT NULL,
    [UserEmail] nvarchar(max) NULL,
    [Type] int NOT NULL,
    [Price] decimal(18,2) NULL,
    [Quantity] int NOT NULL,
    [AvailableTickets] int NOT NULL,
    [Status] int NOT NULL,
    [IssuedAt] datetime2 NULL,
    [ExpiryDate] datetime2 NULL,
    [QRCode] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    [UserId] int NULL,
    CONSTRAINT [PK_Ticket] PRIMARY KEY ([TicketId]),
    CONSTRAINT [FK_Ticket_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Ticket_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
);
GO


CREATE TABLE [Waitlist] (
    [WaitlistId] int NOT NULL IDENTITY,
    [EventId] int NOT NULL,
    [UserId] int NOT NULL,
    [JoinedAt] datetime2 NOT NULL,
    [Notified] bit NOT NULL,
    [UserEmail] nvarchar(max) NULL,
    [EventTitle] nvarchar(max) NULL,
    CONSTRAINT [PK_Waitlist] PRIMARY KEY ([WaitlistId]),
    CONSTRAINT [FK_Waitlist_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Waitlist_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Registration] (
    [RegistrationId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [EventId] int NOT NULL,
    [OccurrenceId] int NOT NULL,
    [TicketId] int NOT NULL,
    [Status] int NOT NULL,
    [PaymentStatus] int NOT NULL,
    [RegisteredAt] datetime2 NOT NULL,
    [QrCode] nvarchar(max) NULL,
    [CheckinTime] datetime2 NULL,
    [EmailReminderSent] bit NOT NULL,
    [PaymentId] int NULL,
    [UserEmail] nvarchar(max) NULL,
    [EventTitle] nvarchar(max) NULL,
    [TicketType] nvarchar(max) NULL,
    [AdminNotes] nvarchar(max) NULL,
    CONSTRAINT [PK_Registration] PRIMARY KEY ([RegistrationId]),
    CONSTRAINT [FK_Registration_EventOccurrence_OccurrenceId] FOREIGN KEY ([OccurrenceId]) REFERENCES [EventOccurrence] ([OccurrenceId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Registration_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Registration_Payment_PaymentId] FOREIGN KEY ([PaymentId]) REFERENCES [Payment] ([PaymentId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Registration_Ticket_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [Ticket] ([TicketId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Registration_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [EventFeedback] (
    [FeedbackId] int NOT NULL IDENTITY,
    [EventId] int NOT NULL,
    [UserId] int NOT NULL,
    [RegistrationId] int NULL,
    [Rating] int NOT NULL,
    [Comments] nvarchar(max) NULL,
    [SubmittedAt] datetime2 NOT NULL,
    [UserEmail] nvarchar(max) NULL,
    [EventTitle] nvarchar(max) NULL,
    CONSTRAINT [PK_EventFeedback] PRIMARY KEY ([FeedbackId]),
    CONSTRAINT [FK_EventFeedback_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EventFeedback_Registration_RegistrationId] FOREIGN KEY ([RegistrationId]) REFERENCES [Registration] ([RegistrationId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EventFeedback_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO


CREATE INDEX [IX_Bookmark_EventId] ON [Bookmark] ([EventId]);
GO


CREATE INDEX [IX_Bookmark_UserId] ON [Bookmark] ([UserId]);
GO


CREATE INDEX [IX_EventFaq_EventId] ON [EventFaq] ([EventId]);
GO


CREATE INDEX [IX_EventFeedback_EventId] ON [EventFeedback] ([EventId]);
GO


CREATE INDEX [IX_EventFeedback_RegistrationId] ON [EventFeedback] ([RegistrationId]);
GO


CREATE INDEX [IX_EventFeedback_UserId] ON [EventFeedback] ([UserId]);
GO


CREATE INDEX [IX_EventInvitation_EventId] ON [EventInvitation] ([EventId]);
GO


CREATE INDEX [IX_EventInvitation_InvitedUserId] ON [EventInvitation] ([InvitedUserId]);
GO


CREATE INDEX [IX_EventLog_EventId] ON [EventLog] ([EventId]);
GO


CREATE INDEX [IX_EventLog_UserId] ON [EventLog] ([UserId]);
GO


CREATE INDEX [IX_EventMedia_EventId] ON [EventMedia] ([EventId]);
GO


CREATE INDEX [IX_EventOccurrence_EventId] ON [EventOccurrence] ([EventId]);
GO


CREATE INDEX [IX_Events_OrganizerId] ON [Events] ([OrganizerId]);
GO


CREATE INDEX [IX_EventSpeaker_EventId] ON [EventSpeaker] ([EventId]);
GO


CREATE INDEX [IX_Notification_EventId] ON [Notification] ([EventId]);
GO


CREATE INDEX [IX_Notification_UserId] ON [Notification] ([UserId]);
GO


CREATE INDEX [IX_Payment_EventId] ON [Payment] ([EventId]);
GO


CREATE INDEX [IX_Payment_EventId1] ON [Payment] ([EventId1]);
GO


CREATE INDEX [IX_Payment_UserId] ON [Payment] ([UserId]);
GO


CREATE INDEX [IX_Payment_UserId1] ON [Payment] ([UserId1]);
GO


CREATE INDEX [IX_Registration_EventId] ON [Registration] ([EventId]);
GO


CREATE INDEX [IX_Registration_OccurrenceId] ON [Registration] ([OccurrenceId]);
GO


CREATE INDEX [IX_Registration_PaymentId] ON [Registration] ([PaymentId]);
GO


CREATE INDEX [IX_Registration_TicketId] ON [Registration] ([TicketId]);
GO


CREATE INDEX [IX_Registration_UserId] ON [Registration] ([UserId]);
GO


CREATE INDEX [IX_Ticket_EventId] ON [Ticket] ([EventId]);
GO


CREATE INDEX [IX_Ticket_UserId] ON [Ticket] ([UserId]);
GO


CREATE INDEX [IX_Waitlist_EventId] ON [Waitlist] ([EventId]);
GO


CREATE INDEX [IX_Waitlist_UserId] ON [Waitlist] ([UserId]);
GO


