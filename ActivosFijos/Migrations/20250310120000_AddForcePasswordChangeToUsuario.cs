using System.Data.Entity.Migrations;

namespace ActivosFijos.Migrations
{
    public partial class AddForcePasswordChangeToUsuario : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PLU_CONF_Usuario", "ForcePasswordChange", c => c.Boolean(nullable: false, defaultValue: false));
        }

        public override void Down()
        {
            DropColumn("dbo.PLU_CONF_Usuario", "ForcePasswordChange");
        }
    }
}
