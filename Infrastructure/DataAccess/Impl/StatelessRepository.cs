using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using log4net;
using NHibernate;
using NHibernate.Linq;
using NHibernateHelper.Core.Interfaces;
using NHibernate.Transform;

namespace NHibernateHelper.Infrastructure.DataAccess.Impl {
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T1"></typeparam>
	public class StatelessRepository<T1> : IRepository<T1>, IDisposable where T1 : DatabaseEngine {
		private static ILog _log = LogManager.GetLogger(typeof(Repository<T1>));
		private IStatelessSession Session { get { return lazySession.Value; } }
		private Lazy<IStatelessSession> lazySession { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sessionFactory"></param>
		public StatelessRepository(ISessionFactory sessionFactory) {
			lazySession = new Lazy<IStatelessSession>(sessionFactory.OpenStatelessSessionExtension);
		}

        public void ExecuteUpdateOrDeleteNamedQuery(string queryName, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            var query = PrepareNamedSQL(queryName, parameters);

            Session.CreateSQLQuery(query).ExecuteUpdate();

            SubmitChanges();
        }




        public IList<T> Execute_storeProc_or_queryFrom_xml<T>(string queryName, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            var query = PrepareNamedSQL(queryName, parameters);

            return Session.CreateSQLQuery(query).SetResultTransformer(Transformers.AliasToBean(typeof(T))).List<T>(); ;
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
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public T FetchFirstOrDefaultRecord<T>(Expression<Func<T, bool>> predicate) {
			return Session.Query<T>().Where(predicate).FirstOrDefault();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public IQueryable<T> Find<T>(Expression<Func<T, bool>> predicate) {
			return Session.Query<T>().Where(predicate);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IQueryable<T> GetAll<T>() {
			return Session.Query<T>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queryName"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public IList<T> ExecuteNamedQuery<T>(string queryName, IEnumerable<KeyValuePair<string, object>> parameters = null) {
			return PrepareNamedQuery(queryName, parameters).List<T>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queryName"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public IList<T> ExecuteNamedQuerySQL<T>(string queryName, IEnumerable<KeyValuePair<string, object>> parameters = null) {
			var query = PrepareNamedSQL(queryName, parameters);

			return Session.CreateSQLQuery(query).List<T>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queryName"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public T ExecuteUniqueNamedQuery<T>(string queryName, IEnumerable<KeyValuePair<string, object>> parameters = null) {
			return PrepareNamedQuery(queryName, parameters).UniqueResult<T>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="queryName"></param>
		/// <param name="parameters"></param>
		public void ExecuteUpdateNamedQuery(string queryName, IEnumerable<KeyValuePair<string, object>> parameters = null) {
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			try {
				var query = PrepareNamedSQL(queryName, parameters);

				Session.CreateSQLQuery(query).ExecuteUpdate();
				
				SubmitChanges();
			}
			catch {
				throw;
			}
			finally {
				stopwatch.Stop();
				_log.InfoFormat("ExecuteUpdateNamedQuery({0}) Elapsed time: {1:G};", queryName, stopwatch.Elapsed);
			}
		}

       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string PrepareNamedSQL(string queryName, IEnumerable<KeyValuePair<string, object>> parameters = null) {
			var query = Session.GetNamedQuery(queryName).QueryString;

			foreach (var parameter in (parameters ?? Enumerable.Empty<KeyValuePair<string, object>>())) {
				var enumerable = parameter.Value as IEnumerable;
				
				if (enumerable != null && enumerable.GetType() != typeof(string)) {
					var oldVal = string.Format(":{0}", parameter.Key);
					var newVal = string.Join(", ", enumerable.Cast<object>().Select(x => x.ToString()).ToArray());
					query = query.Replace(oldVal, newVal);
				}
				else {
					query = query.Replace(string.Format(":{0}", parameter.Key), PrepareValue(parameter.Value));
				}
			}

			return query;
		}

        public IList<T> ExecuteRawUnmappedSql<T>(string sql)
        {
            return Session.CreateSQLQuery(sql).SetResultTransformer(Transformers.AliasToBean(typeof(T))).List<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string PrepareValue(object value) {
			if (value.GetType() == typeof(DateTime)) {
				return string.Format("1{0:yyMMdd}", value);
			}
			else {
				return string.Format("'{0}'", value.ToString());
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="queryName"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		private IQuery PrepareNamedQuery(string queryName, IEnumerable<KeyValuePair<string, object>> parameters = null) {
			var query = Session.GetNamedQuery(queryName);

			foreach (var parameter in (parameters ?? Enumerable.Empty<KeyValuePair<string, object>>())) {
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
		/// <typeparam name="T"></typeparam>
		/// <param name="queryName"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public IList<T> ExecuteStoredProcedure<T>(string queryName, params object[] parameters) {
			return Session.GetNamedQuery(queryName)
				.SetParameterList("parameters", parameters)
				.List<T>();
		}


		/// <summary>
		/// 
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
		/// 
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
		/// <param name="queryName"></param>
		/// <param name="parameters"></param>
		public void ExecuteNonQuery(string queryName, params object[] parameters) {
			Session.GetNamedQuery(queryName)
				.SetParameterList("parameters", parameters).ExecuteUpdate();
			
			SubmitChanges();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity"></param>
		/// <returns></returns>
		public T SaveOrUpdate<T>(T entity) {

             

            Session.Update(entity);
			
			return entity;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity"></param>
		/// <returns></returns>
		public T Insert<T>(T entity) {
			Session.Insert(entity);
			
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
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity"></param>
		public void Delete<T>(T entity) where T : Core.Interfaces.IEntity {
			if (entity != null)
				Session.Delete(entity);
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
	}
}