namespace ReadingListPlus.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Maketitlenotrequired : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Cards", "Title", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Cards", "Title", c => c.String(nullable: false));
        }
    }
}
