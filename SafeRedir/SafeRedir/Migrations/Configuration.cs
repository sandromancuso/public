using System.Data.Entity.Migrations;
using Renfield.SafeRedir.Data;

namespace Renfield.SafeRedir.Migrations
{
  internal sealed class Configuration : DbMigrationsConfiguration<Database>
  {
    public Configuration()
    {
      AutomaticMigrationsEnabled = true;
    }

    protected override void Seed(Database context)
    {
      //
    }
  }
}