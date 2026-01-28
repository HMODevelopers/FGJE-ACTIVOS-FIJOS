using System.Data.Entity.Migrations;

namespace ActivosFijos.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<ActivosFijos.Models.ModelContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }
    }
}
