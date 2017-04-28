using BlogSystem.Models;
using BlogSystem.Migrations;
using Microsoft.Owin;
using Owin;
using System.Configuration;
using System.Data.Entity;

[assembly: OwinStartupAttribute(typeof(BlogSystem.Startup))]
namespace BlogSystem
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<BlogDbContext, Migrations.Configuration>());

            ConfigureAuth(app);
        }
    }
}
