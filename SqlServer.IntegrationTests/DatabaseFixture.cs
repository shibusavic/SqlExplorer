using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SqlServer.IntegrationTests
{
    public class DatabaseFixture : IDisposable
    {
        private readonly IConfiguration configuration;

        public DatabaseFixture()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            configuration = builder.Build();

            Database = DatabaseFactory.CreateAsync(ConnectionString).GetAwaiter().GetResult();
        }

        public string ConnectionString => configuration.GetConnectionString("Books");

        public Database Database { get; }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
