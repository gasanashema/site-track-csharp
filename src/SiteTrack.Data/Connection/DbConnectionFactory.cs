using System;
using System.Data;
using System.Data.Common;
using System.Configuration;

namespace SiteTrack.Data.Connection
{
    public interface IDbConnectionFactory
    {
        string ProviderName { get; }
        string ConnectionString { get; }
        IDbConnection CreateConnection();
        bool TestConnection();
    }

    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;
        private readonly string _providerName;

        public string ProviderName
        {
            get { return _providerName; }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public DbConnectionFactory()
        {
            // Check App.config for configured connection string, or default to internal portable provider
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["SiteTrackDb"];
            if (settings != null && !string.IsNullOrEmpty(settings.ConnectionString))
            {
                this._connectionString = settings.ConnectionString;
                this._providerName = settings.ProviderName ?? "System.Data.SqlClient";
            }
            else
            {
                this._providerName = "SiteTrack.Internal";
                this._connectionString = "Data Source=sitetrack_portable.db;";
            }
        }

        public DbConnectionFactory(string connectionString, string providerName)
        {
            this._connectionString = connectionString;
            this._providerName = providerName;
        }

        public IDbConnection CreateConnection()
        {
            if (string.Equals(this._providerName, "SiteTrack.Internal", StringComparison.OrdinalIgnoreCase))
            {
                return SiteTrackDataStore.Instance.CreateConnection();
            }

            try
            {
                DbProviderFactory factory = DbProviderFactories.GetFactory(this._providerName);
                DbConnection connection = factory.CreateConnection();
                if (connection != null)
                {
                    connection.ConnectionString = this._connectionString;
                }
                return connection;
            }
            catch (Exception)
            {
                // Fall back to internal data store
                return SiteTrackDataStore.Instance.CreateConnection();
            }
        }

        public bool TestConnection()
        {
            try
            {
                using (IDbConnection conn = CreateConnection())
                {
                    conn.Open();
                    return conn.State == ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}

