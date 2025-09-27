using FluentMigrator;

namespace CronductorApp.Data.Migrations;

[Migration(000001)]
public class InitDb : Migration
{
    public override void Up()
    {
        Create.Table("Requests"); //todo - flesh out schema
    }

    public override void Down()
    {
        Delete.Table("Requests");
    }
}