using FluentMigrator;

namespace CronductorApp.Data.Migrations;

[Migration(000001)]
public class CreateRequestDefinitionTable  : Migration
{
    public override void Up()
    {
        Create.Table("RequestDefinitions")
            .WithColumn("Id").AsString(36).PrimaryKey().NotNullable()
            .WithColumn("Version").AsInt32().NotNullable()
            .WithColumn("Name").AsString(255).NotNullable()
            .WithColumn("Method").AsString(10).NotNullable()
            .WithColumn("Url").AsString(2048).NotNullable()
            .WithColumn("ContentType").AsString(255).Nullable()
            .WithColumn("Headers").AsString().Nullable() // JSON serialized
            .WithColumn("Body").AsString().Nullable() // JSON serialized
            .WithColumn("CronSchedule").AsString(100).NotNullable()
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithColumn("LastExecuted").AsDateTime().Nullable();
        
        Insert.IntoTable("RequestDefinitions").Row(new
        {
            Id = Guid.NewGuid().ToString(),
            Version = 1,
            Name = "Sample Request",
            Method = "GET",
            Url = "https://jsonplaceholder.typicode.com/todos/1",
            ContentType = string.Empty,
            Headers = string.Empty,
            Body = string.Empty,
            CronSchedule = "* */5 * * * *", // Every 5 minutes
            IsActive = true,
            CreatedAt = DateTime.Now,
            LastExecuted = (DateTime?)null
        });
    }

    public override void Down()
    {
        Delete.Table("RequestDefinitions");
    }
}