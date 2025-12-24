using Microsoft.EntityFrameworkCore;
using OneAI.Entities;

namespace OneAI.Data;

/// <summary>
/// 应用数据库上下文
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// AI 账户表
    /// </summary>
    public DbSet<AIAccount> AIAccounts { get; set; }

    /// <summary>
    /// 系统设置表
    /// </summary>
    public DbSet<SystemSettings> SystemSettings { get; set; }

    /// <summary>
    /// AI请求日志表
    /// </summary>
    public DbSet<AIRequestLog> AIRequestLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置 AIAccount 实体
        modelBuilder.Entity<AIAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Provider).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ApiKey).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.BaseUrl).HasMaxLength(500);
            entity.Property(e => e.OAuthToken).HasMaxLength(4000); // OAuth 数据（JSON 格式）

            // 创建索引以提高查询性能
            entity.HasIndex(e => e.Provider);
            entity.HasIndex(e => e.IsEnabled);
            entity.HasIndex(e => e.IsRateLimited);
        });

        // 配置 SystemSettings 实体
        modelBuilder.Entity<SystemSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Value).HasMaxLength(4000);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DataType).HasMaxLength(50).IsRequired();

            // 创建唯一索引确保 Key 唯一性
            entity.HasIndex(e => e.Key).IsUnique();
        });

    }
}