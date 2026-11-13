using System;
using System.Data;
using SiteTrack.Data.Connection;

namespace SiteTrack.Data
{
    public class DbHelper
    {
        private readonly IDbConnectionFactory _factory;

        public DbHelper(IDbConnectionFactory factory)
        {
            this._factory = factory ?? new DbConnectionFactory();
        }

        public int ExecuteNonQuery(string commandText, CommandType commandType = CommandType.Text, params IDbDataParameter[] parameters)
        {
            using (IDbConnection conn = _factory.CreateConnection())
            {
                conn.Open();
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = commandText;
                    cmd.CommandType = commandType;
                    if (parameters != null)
                    {
                        foreach (IDbDataParameter param in parameters)
                        {
                            cmd.Parameters.Add(param);
                        }
                    }
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public object ExecuteScalar(string commandText, CommandType commandType = CommandType.Text, params IDbDataParameter[] parameters)
        {
            using (IDbConnection conn = _factory.CreateConnection())
            {
                conn.Open();
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = commandText;
                    cmd.CommandType = commandType;
                    if (parameters != null)
                    {
                        foreach (IDbDataParameter param in parameters)
                        {
                            cmd.Parameters.Add(param);
                        }
                    }
                    return cmd.ExecuteScalar();
                }
            }
        }

        public IDataReader ExecuteReader(string commandText, CommandType commandType = CommandType.Text, params IDbDataParameter[] parameters)
        {
            IDbConnection conn = _factory.CreateConnection();
            conn.Open();
            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = commandText;
                cmd.CommandType = commandType;
                if (parameters != null)
                {
                    foreach (IDbDataParameter param in parameters)
                    {
                        cmd.Parameters.Add(param);
                    }
                }
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
        }
    }
}

