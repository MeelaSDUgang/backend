using GatewayApi.Entities;
using GatewayApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace GatewayApi.Data;

public class AppDbContext : DbContext
{
    private static readonly DateTimeOffset SeedCreatedAt = new(2026, 5, 30, 10, 30, 0, TimeSpan.Zero);
    private static readonly Guid SeedUser1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SeedUser2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid SeedUser3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid SeedOperation1Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
    private static readonly Guid SeedOperation3Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3");
    private static readonly Guid SeedCase2Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc2");
    private static readonly Guid SeedCase3Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc3");

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Operation> Operations => Set<Operation>();

    public DbSet<AppealCase> AppealCases => Set<AppealCase>();

    public DbSet<AppealAnswer> AppealAnswers => Set<AppealAnswer>();

    public DbSet<AppealDocument> AppealDocuments => Set<AppealDocument>();

    public DbSet<SupportDecision> SupportDecisions => Set<SupportDecision>();

    public DbSet<BankAdapter> BankAdapters { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.FullName).IsRequired().HasMaxLength(200);

            entity.HasIndex(m => m.ApiKey).IsUnique();
            entity.Property(m => m.ApiKey).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<BankAdapter>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.RoutingKey).IsRequired().HasMaxLength(50);
            entity.Property(b => b.Name).IsRequired().HasMaxLength(100);
            entity.Property(b => b.SupportedGatewayTypes).HasMaxLength(50);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Amount).HasPrecision(18, 2);
            entity.Property(t => t.NameDest).IsRequired().HasMaxLength(100);
            entity.Property(t => t.NameOrig).IsRequired().HasMaxLength(100);
            entity.Property(t => t.NewbalanceDest).HasPrecision(18, 2);
            entity.Property(t => t.NewbalanceOrig).HasPrecision(18, 2);
            entity.Property(t => t.OldbalanceDest).HasPrecision(18, 2);
            entity.Property(t => t.OldbalanceOrg).HasPrecision(18, 2);
            entity.Property(t => t.Type).IsRequired().HasMaxLength(32);
            entity.Property(t => t.Label).IsRequired().HasMaxLength(100);
            entity.Property(t => t.Currency).HasMaxLength(3);

            entity.HasIndex(t => new { t.IdempotencyKey, t.UserId }).IsUnique();

            entity.Property(t => t.GatewayType)
                .HasConversion<string>();

            entity.Property(t => t.TransactionStatus)
                .HasConversion<string>();

            entity.Property(t => t.FailureReason)
                .HasMaxLength(500);

            entity.HasOne(t => t.User)
                .WithMany(m => m.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.BankAdapter)
                .WithMany(b => b.Transactions)
                .HasForeignKey(t => t.BankId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        ConfigureUsers(modelBuilder);
        ConfigureOperations(modelBuilder);
        ConfigureAppealCases(modelBuilder);
        ConfigureAppealAnswers(modelBuilder);
        ConfigureAppealDocuments(modelBuilder);
        ConfigureSupportDecisions(modelBuilder);
        SeedDemoData(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder builder)
    {
        builder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);

            entity.Property(user => user.Id).HasColumnName("id");
            entity.Property(user => user.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
            entity.Property(user => user.Phone).HasColumnName("phone").HasMaxLength(32).IsRequired();
            entity.Property(user => user.AccountStatus)
                .HasColumnName("account_status")
                .HasConversion<string>()
                .HasMaxLength(16)
                .IsRequired();
            entity.Property(user => user.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();
            entity.Property(user => user.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.HasIndex(user => user.Phone).IsUnique();
        });
    }

    private static void ConfigureOperations(ModelBuilder builder)
    {
        builder.Entity<Operation>(entity =>
        {
            entity.ToTable("operations");
            entity.HasKey(operation => operation.Id);

            entity.Property(operation => operation.Id).HasColumnName("id");
            entity.Property(operation => operation.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(operation => operation.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
            entity.Property(operation => operation.Currency)
                .HasColumnName("currency")
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired();
            entity.Property(operation => operation.RecipientName)
                .HasColumnName("recipient_name")
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(operation => operation.RecipientAccount)
                .HasColumnName("recipient_account")
                .HasMaxLength(64);
            entity.Property(operation => operation.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(operation => operation.BlockReasonCode)
                .HasColumnName("block_reason_code")
                .HasConversion<string>()
                .HasMaxLength(64)
                .IsRequired();
            entity.Property(operation => operation.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();
            entity.Property(operation => operation.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.HasOne(operation => operation.User)
                .WithMany(user => user.Operations)
                .HasForeignKey(operation => operation.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(operation => operation.UserId);
            entity.HasIndex(operation => operation.Status);
        });
    }

    private static void ConfigureAppealCases(ModelBuilder builder)
    {
        builder.Entity<AppealCase>(entity =>
        {
            entity.ToTable("appeal_cases");
            entity.HasKey(appealCase => appealCase.Id);

            entity.Property(appealCase => appealCase.Id).HasColumnName("id");
            entity.Property(appealCase => appealCase.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(appealCase => appealCase.OperationId).HasColumnName("operation_id");
            entity.Property(appealCase => appealCase.CaseType)
                .HasColumnName("case_type")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(appealCase => appealCase.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(appealCase => appealCase.SupportSummary)
                .HasColumnName("support_summary")
                .HasMaxLength(4000);
            entity.Property(appealCase => appealCase.ClientMessage)
                .HasColumnName("client_message")
                .HasMaxLength(2000);
            entity.Property(appealCase => appealCase.MissingInfoJson)
                .HasColumnName("missing_info_json")
                .HasColumnType("jsonb");
            entity.Property(appealCase => appealCase.RouteTo)
                .HasColumnName("route_to")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(appealCase => appealCase.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();
            entity.Property(appealCase => appealCase.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.HasOne(appealCase => appealCase.User)
                .WithMany(user => user.AppealCases)
                .HasForeignKey(appealCase => appealCase.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(appealCase => appealCase.Operation)
                .WithMany(operation => operation.AppealCases)
                .HasForeignKey(appealCase => appealCase.OperationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(appealCase => appealCase.UserId);
            entity.HasIndex(appealCase => appealCase.OperationId);
            entity.HasIndex(appealCase => appealCase.Status);
            entity.HasIndex(appealCase => appealCase.RouteTo);
        });
    }

    private static void ConfigureAppealAnswers(ModelBuilder builder)
    {
        builder.Entity<AppealAnswer>(entity =>
        {
            entity.ToTable("appeal_answers");
            entity.HasKey(answer => answer.Id);

            entity.Property(answer => answer.Id).HasColumnName("id");
            entity.Property(answer => answer.CaseId).HasColumnName("case_id").IsRequired();
            entity.Property(answer => answer.QuestionKey)
                .HasColumnName("question_key")
                .HasMaxLength(128)
                .IsRequired();
            entity.Property(answer => answer.QuestionText)
                .HasColumnName("question_text")
                .HasMaxLength(1000)
                .IsRequired();
            entity.Property(answer => answer.Answer)
                .HasColumnName("answer")
                .HasMaxLength(4000)
                .IsRequired();
            entity.Property(answer => answer.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.HasOne(answer => answer.Case)
                .WithMany(appealCase => appealCase.Answers)
                .HasForeignKey(answer => answer.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(answer => answer.CaseId);
            entity.HasIndex(answer => new { answer.CaseId, answer.QuestionKey }).IsUnique();
        });
    }

    private static void ConfigureAppealDocuments(ModelBuilder builder)
    {
        builder.Entity<AppealDocument>(entity =>
        {
            entity.ToTable("appeal_documents");
            entity.HasKey(document => document.Id);

            entity.Property(document => document.Id).HasColumnName("id");
            entity.Property(document => document.CaseId).HasColumnName("case_id").IsRequired();
            entity.Property(document => document.DocumentType)
                .HasColumnName("document_type")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(document => document.FileName)
                .HasColumnName("file_name")
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(document => document.MockUrl)
                .HasColumnName("mock_url")
                .HasMaxLength(2048);
            entity.Property(document => document.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.HasOne(document => document.Case)
                .WithMany(appealCase => appealCase.Documents)
                .HasForeignKey(document => document.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(document => document.CaseId);
        });
    }

    private static void ConfigureSupportDecisions(ModelBuilder builder)
    {
        builder.Entity<SupportDecision>(entity =>
        {
            entity.ToTable("support_decisions");
            entity.HasKey(decision => decision.Id);

            entity.Property(decision => decision.Id).HasColumnName("id");
            entity.Property(decision => decision.CaseId).HasColumnName("case_id").IsRequired();
            entity.Property(decision => decision.Decision)
                .HasColumnName("decision")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(decision => decision.Comment)
                .HasColumnName("comment")
                .HasMaxLength(2000);
            entity.Property(decision => decision.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.HasOne(decision => decision.Case)
                .WithMany(appealCase => appealCase.Decisions)
                .HasForeignKey(decision => decision.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(decision => decision.CaseId);
        });
    }

    private static void SeedDemoData(ModelBuilder builder)
    {
        builder.Entity<User>().HasData(
            new
            {
                Id = SeedUser1Id,
                FullName = "Andrey K.",
                Phone = "+7 777 000 00 00",
                ApiKey = "demo-user-1-api-key",
                SecretKeyHash = "demo-user-1-secret-hash",
                AccountStatus = AccountStatus.LIMITED,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = SeedCreatedAt
            },
            new
            {
                Id = SeedUser2Id,
                FullName = "Client Account Appeal",
                Phone = "+7 777 000 00 02",
                ApiKey = "demo-user-2-api-key",
                SecretKeyHash = "demo-user-2-secret-hash",
                AccountStatus = AccountStatus.LIMITED,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = SeedCreatedAt
            },
            new
            {
                Id = SeedUser3Id,
                FullName = "Client Operation Confirmation",
                Phone = "+7 777 000 00 03",
                ApiKey = "demo-user-3-api-key",
                SecretKeyHash = "demo-user-3-secret-hash",
                AccountStatus = AccountStatus.ACTIVE,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = SeedCreatedAt
            });

        builder.Entity<Operation>().HasData(
            new
            {
                Id = SeedOperation1Id,
                UserId = SeedUser1Id,
                Amount = 250000m,
                Currency = Currency.KZT,
                RecipientName = "Alisher M.",
                RecipientAccount = "KZ00 **** **** 1234",
                Status = OperationStatus.PENDING_CONFIRMATION,
                BlockReasonCode = BlockReasonCode.CLIENT_CONFIRMATION_REQUIRED,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = SeedCreatedAt
            },
            new
            {
                Id = SeedOperation3Id,
                UserId = SeedUser3Id,
                Amount = 45000m,
                Currency = Currency.KZT,
                RecipientName = "Service Company",
                RecipientAccount = "KZ00 **** **** 9876",
                Status = OperationStatus.PENDING_CONFIRMATION,
                BlockReasonCode = BlockReasonCode.CLIENT_CONFIRMATION_REQUIRED,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = SeedCreatedAt
            });

        builder.Entity<AppealCase>().HasData(
            new
            {
                Id = SeedCase2Id,
                UserId = SeedUser2Id,
                OperationId = (Guid?)null,
                CaseType = AppealCaseType.ACCOUNT_BLOCK_APPEAL,
                Status = AppealCaseStatus.NEED_MORE_INFO,
                SupportSummary =
                    "Client reported account restriction after several incoming transfers. Supporting documents are not attached yet.",
                ClientMessage = (string?)null,
                MissingInfoJson =
                    "[\"Source of funds confirmation is required\", \"Purpose of incoming transfers is required\"]",
                RouteTo = RouteTo.COMPLIANCE,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = SeedCreatedAt
            },
            new
            {
                Id = SeedCase3Id,
                UserId = SeedUser3Id,
                OperationId = (Guid?)SeedOperation3Id,
                CaseType = AppealCaseType.OPERATION_CONFIRMATION,
                Status = AppealCaseStatus.SUBMITTED,
                SupportSummary =
                    "Client confirmed service payment. Recipient is a company/service. Payment check is attached.",
                ClientMessage = (string?)null,
                MissingInfoJson = "[]",
                RouteTo = RouteTo.SUPPORT,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = SeedCreatedAt
            });
    }
}