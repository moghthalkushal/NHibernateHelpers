using System.Data;
using NHibernate.Connection;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;

namespace NHibernateHelper.Infrastructure.DataAccess {
	/// <summary>
	/// This class solely exists because the database has 'set nocount on'.  This causes Nhibernate to break because
	/// it expects 'nonquery's to return the number of rows affected to determine success.  With this setting off, 
	/// the database will return -1, ergo breaking Nhibernate's default configuration.
	/// </summary>
	internal class SqlServerNoCountOffConnectionProvider : ConnectionProvider {
		/// <summary>
		/// Adds the 'set nocount off' statement prior to all statements sent to the server.
		/// </summary>
		/// <returns></returns>
		public override IDbConnection GetConnection() {
			IDbConnection conn = Driver.CreateConnection();
			conn.ConnectionString = ConnectionString;
			conn.Open();

			System.Data.IDbCommand cmd = Driver.GenerateCommand(CommandType.Text, new SqlString("SET NOCOUNT OFF"), new SqlType[] { });

			using (cmd) {
				cmd.Connection = conn;
				cmd.ExecuteNonQuery();
			}
			
			return conn;
		}
	}
}
