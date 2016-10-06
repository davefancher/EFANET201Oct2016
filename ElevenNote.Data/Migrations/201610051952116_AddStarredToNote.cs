namespace ElevenNote.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStarredToNote : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Note", "Starred", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Note", "Starred");
        }
    }
}
