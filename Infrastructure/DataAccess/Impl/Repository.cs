using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using log4net;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Transform;

namespace NHibernateHelper.Infrastructure.DataAccess.Impl {
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T1"></typeparam>
	public class Repository<T1> : IRepository<T1>, IDisposable where T1 : DatabaseEngine {
		private static ILog _log = LogManager.GetLogger(typeof(Repository<T1>));
		private ISession Session { get { return lazySession.Value; } }
		private Lazy<ISession> lazySession { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sessionFactory"></param>
		public Repository(ISessionFactory sessionFactory) {
			lazySession = new Lazy<ISession>(sessionFactory.OpenSessionExtension);
		}

		/// <summary>
		/// Gets First or Default value from the list
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public T FetchFirstOrDefaultRecord<T>(Expression<Func<T, bool>> predicate) {
			return Session.Query<T>().Where(predicate).FirstOrDefault();
		}

        /// <summary>
        /// Gets All the records with the specified where condition
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IQueryable<T> Find<T>(Expression<Func<T, bool>> predicate) {
			return Session.Query<T>().Where(predicate);
		}

		/// <summary>
		/// Find 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IQueryable<T> GetAll<T>() {
			return Session.Query<T>();
		}

		
		/// <summary>
		/// Can execute a query / stored proc from an hbm.xml file 
        /// Make sure that .hbm file properties are always
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queryName"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public IList<T> Execute_storeProc_or_queryFrom_xml<T>(string queryName, IEnumerable<KeyValuePair<string, object>> parameters = null) {
			var query = PrepareNamedSQL(queryName, parameters);

			return Session.CreateSQLQuery(query).SetResultTransformer(Transformers.AliasToBean(typeof(T))).List<T>(); ;
		}

		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="queryName"></param>
		/// <param name="parameters"></param>
		public void ExecuteUpdateOrDeleteNamedQuery(string queryName, IEnumerable<KeyValuePair<string, object>> parameters = null) {
            PrepareNamedQuery(queryName, parameters).ExecuteUpdate();
        }

        /// <summary>
		/// Execute a sql query which is passed as a string
		/// </summary>
		/// <param name="SQL Query"></param>
		
        public IList<T> ExecuteRawUnmappedSql<T>(string sql)
        {

            return Session.CreateSQLQuery(sql).SetResultTransformer(Transformers.AliasToBean(typeof(T))).List<T>();
        }


       

        private IQuery PrepareNamedQuery(string queryName, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            var query = Session.GetNamedQuery(queryName);

            foreach (var parameter in (parameters ?? Enumerable.Empty<KeyValuePair<string, object>>()))
            {
                var enumerable = parameter.Value as IEnumerable;
                if (enumerable != null && enumerable.GetType() != typeof(string))
                    query.SetParameterList(parameter.Key, enumerable);
                else
                    query.SetParameter(parameter.Key, parameter.Value);
            }

            return query;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string PrepareNamedSQL(string queryName, IEnumerable<KeyValuePair<string, object>> parameters = null) {
			var query = Session.GetNamedQuery(queryName).ToString();

			foreach (var parameter in (parameters ?? Enumerable.Empty<KeyValuePair<string, object>>())) {
				var enumerable = parameter.Value as IEnumerable;
				
				if (enumerable != null && enumerable.GetType() != typeof(string)) {
					var oldVal = string.Format(":{0}", parameter.Key);
					var newVal = string.Join(", ", enumerable.Cast<object>().Select(x => x.ToString()).ToArray());
					query = query.Replace(oldVal, newVal);
				}
				else {
					query = query.Replace(string.Format(":{0}", parameter.Key), string.Format("'{0}'", parameter.Value.ToString()));
				}
			}

			return query;
		}

		



	

		/// <summary>
		/// Execute a stored procedure that returns a single value
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queryName"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public T ExecuteScalarStoredProcedure<T>(string queryName, params object[] parameters) {
			return Session.GetNamedQuery(queryName)
				.SetParameterList("parameters", parameters)
				.UniqueResult<T>();
		}

        /// <summary>
        ///  Execute a stored procedure that returns a single value , without any parmeters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryName"></param>
        /// <returns></returns>
        public T ExecuteScalarStoredProcedure<T>(string queryName) {
			return Session.GetNamedQuery(queryName)
				.UniqueResult<T>();
		}

	

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity"></param>
		/// <returns></returns>
		public T SaveOrUpdate<T>(T entity) {
			Session.SaveOrUpdate(entity);
			
			return entity;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity"></param>
		/// <returns></returns>
		public T Insert<T>(T entity) {
			Session.Save(entity);
			
			return entity;
		}

		/// <summary>
		/// 
		/// </summary>
		public void SubmitChanges() {
			try {
				Session.Transaction.Commit();
				Session.BeginTransaction();
			}
			catch {
				Session.Transaction.Rollback();
				throw;
			}
		}

		/// <summary>
		/// Delete the entity , for this we need to write a map
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity"></param>
		public void Delete<T>(T entity) where T : Core.Interfaces.IEntity {
			if (entity != null)
				Session.Delete(entity);
		}

        public void DropTable(string tableName)
        {
            try
            {
                if (!Session.Transaction.IsActive)
                    Session.Transaction.Begin();
                Session.CreateSQLQuery("DROP TABLE " + tableName + ";").ExecuteUpdate();
                SubmitChanges();
            }
            catch
            {
                Session.Transaction.Rollback();
                Session.Transaction.Begin();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (Session != null) {
					using (Session) {
						if (Session.Transaction.IsActive) { Session.Transaction.Rollback(); }
					}

					Session.Dispose();
				}

				if (lazySession.IsValueCreated) {
					lazySession.Value.Dispose();
				}
			}
		}

       
         public IList<T> ExecuteNamedQuery<T>(string queryName, IEnumerable<KeyValuePair<string, object>> parameters = null) {
            return PrepareNamedQuery(queryName, parameters).List<T>();
        }
    }
}