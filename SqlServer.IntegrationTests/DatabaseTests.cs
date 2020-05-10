using System;
using System.Threading.Tasks;
using Xunit;

namespace SqlServer.IntegrationTests
{
    public class DatabaseTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture fixture;
        public DatabaseTests(DatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task GetTablesAsync()
        {
            var service = fixture.Database;

            var tables = await service.GetTablesAsync();

            Assert.NotEmpty(tables);
        }

        [Fact]
        public async Task GetForeignKeysAsync()
        {
            var service = fixture.Database;
            var fks = await service.GetForeignKeysAsync();

            Assert.NotEmpty(fks);
        }

        [Fact]
        public async Task GetViewsAsync()
        {
            var service = fixture.Database;
            var views = await service.GetViewsAsync();
            Assert.NotEmpty(views);
        }

        [Fact]
        public async Task GetRoutinesAsync()
        {
            var service = fixture.Database;
            var routines = await service.GetRoutinesAsync();
            Assert.NotEmpty(routines);
        }
    }
}
