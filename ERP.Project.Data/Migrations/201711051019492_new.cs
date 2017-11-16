namespace ERP.Project.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _new : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrackerItem", "Subject", c => c.String());
            DropColumn("dbo.TrackerItem", "Subjet");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TrackerItem", "Subjet", c => c.String());
            DropColumn("dbo.TrackerItem", "Subject");
        }
    }
}
