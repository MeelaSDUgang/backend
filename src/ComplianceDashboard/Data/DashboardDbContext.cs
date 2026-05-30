using ComplianceDashboard.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplianceDashboard.Data;

public partial class DashboardDbContext : DbContext
{
    public DashboardDbContext(DbContextOptions<DashboardDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppealAnswer> AppealAnswers { get; set; }

    public virtual DbSet<AppealCase> AppealCases { get; set; }

    public virtual DbSet<AppealDocument> AppealDocuments { get; set; }

    public virtual DbSet<BankAdapter> BankAdapters { get; set; }

    public virtual DbSet<Merchant> Merchants { get; set; }

    public virtual DbSet<Operation> Operations { get; set; }

    public virtual DbSet<SupportDecision> SupportDecisions { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppealAnswer>(entity =>
        {
            entity.ToTable("appeal_answers");

            entity.HasIndex(e => e.CaseId, "IX_appeal_answers_case_id");

            entity.HasIndex(e => new { e.CaseId, e.QuestionKey }, "IX_appeal_answers_case_id_question_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Answer)
                .HasMaxLength(4000)
                .HasColumnName("answer");
            entity.Property(e => e.CaseId).HasColumnName("case_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.QuestionKey)
                .HasMaxLength(128)
                .HasColumnName("question_key");
            entity.Property(e => e.QuestionText)
                .HasMaxLength(1000)
                .HasColumnName("question_text");

            entity.HasOne(d => d.Case).WithMany(p => p.AppealAnswers).HasForeignKey(d => d.CaseId);
        });

        modelBuilder.Entity<AppealCase>(entity =>
        {
            entity.ToTable("appeal_cases");

            entity.HasIndex(e => e.OperationId, "IX_appeal_cases_operation_id");

            entity.HasIndex(e => e.RouteTo, "IX_appeal_cases_route_to");

            entity.HasIndex(e => e.Status, "IX_appeal_cases_status");

            entity.HasIndex(e => e.UserId, "IX_appeal_cases_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CaseType)
                .HasMaxLength(32)
                .HasColumnName("case_type");
            entity.Property(e => e.ClientMessage)
                .HasMaxLength(2000)
                .HasColumnName("client_message");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.MissingInfoJson)
                .HasColumnType("jsonb")
                .HasColumnName("missing_info_json");
            entity.Property(e => e.OperationId).HasColumnName("operation_id");
            entity.Property(e => e.RouteTo)
                .HasMaxLength(32)
                .HasColumnName("route_to");
            entity.Property(e => e.Status)
                .HasMaxLength(32)
                .HasColumnName("status");
            entity.Property(e => e.SupportSummary)
                .HasMaxLength(4000)
                .HasColumnName("support_summary");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Operation).WithMany(p => p.AppealCases)
                .HasForeignKey(d => d.OperationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.User).WithMany(p => p.AppealCases)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AppealDocument>(entity =>
        {
            entity.ToTable("appeal_documents");

            entity.HasIndex(e => e.CaseId, "IX_appeal_documents_case_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CaseId).HasColumnName("case_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.DocumentType)
                .HasMaxLength(32)
                .HasColumnName("document_type");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.MockUrl)
                .HasMaxLength(2048)
                .HasColumnName("mock_url");

            entity.HasOne(d => d.Case).WithMany(p => p.AppealDocuments).HasForeignKey(d => d.CaseId);
        });

        modelBuilder.Entity<BankAdapter>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.RoutingKey).HasMaxLength(50);
            entity.Property(e => e.SupportedGatewayTypes).HasMaxLength(50);
        });

        modelBuilder.Entity<Merchant>(entity =>
        {
            entity.HasIndex(e => e.ApiKey, "IX_Merchants_ApiKey").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ApiKey).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<Operation>(entity =>
        {
            entity.ToTable("operations");

            entity.HasIndex(e => e.Status, "IX_operations_status");

            entity.HasIndex(e => e.UserId, "IX_operations_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.BlockReasonCode)
                .HasMaxLength(64)
                .HasColumnName("block_reason_code");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasColumnName("currency");
            entity.Property(e => e.RecipientAccount)
                .HasMaxLength(64)
                .HasColumnName("recipient_account");
            entity.Property(e => e.RecipientName)
                .HasMaxLength(200)
                .HasColumnName("recipient_name");
            entity.Property(e => e.Status)
                .HasMaxLength(32)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Operations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SupportDecision>(entity =>
        {
            entity.ToTable("support_decisions");

            entity.HasIndex(e => e.CaseId, "IX_support_decisions_case_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CaseId).HasColumnName("case_id");
            entity.Property(e => e.Comment)
                .HasMaxLength(2000)
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Decision)
                .HasMaxLength(32)
                .HasColumnName("decision");

            entity.HasOne(d => d.Case).WithMany(p => p.SupportDecisions).HasForeignKey(d => d.CaseId);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasIndex(e => e.BankId, "IX_Transactions_BankId");

            entity.HasIndex(e => new { e.IdempotencyKey, e.MerchantId }, "IX_Transactions_IdempotencyKey_MerchantId")
                .IsUnique();

            entity.HasIndex(e => e.MerchantId, "IX_Transactions_MerchantId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.BankReferenceId).HasMaxLength(100);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.Property(e => e.RawPayload).HasColumnType("jsonb");

            entity.HasOne(d => d.Bank).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.BankId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Merchant).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.MerchantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasIndex(e => e.Phone, "IX_users_phone").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountStatus)
                .HasMaxLength(16)
                .HasColumnName("account_status");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.FullName)
                .HasMaxLength(200)
                .HasColumnName("full_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(32)
                .HasColumnName("phone");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}