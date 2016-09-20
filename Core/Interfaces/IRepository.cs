using NHibernateHelper.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NHibernateHelper.Core.Domain.Interfaces {
    public interface IRepository {
        T FetchFirstOrDefaultRecord<T>(Expression<Func<T, bool>> predicate);
        IQueryable<T> Find<T>(Expression<Func<T, bool>> predicate);
        IQueryable<T> GetAll<T>();

        void DropTable(string tableName);

        void ExecuteUpdateOrDeleteNamedQuery(string queryName, IEnumerable<KeyValuePair<string, object>> parameters = null);

        IList<T> Execute_storeProc_or_queryFrom_xml<T>(string queryName, IEnumerable<KeyValuePair<string, object>> parameters = null);

        IList<T> ExecuteNamedQuery<T>(string queryName, IEnumerable<KeyValuePair<string, object>> parameters = null);

        IList<T> ExecuteRawUnmappedSql<T>(string sql);
        T ExecuteScalarStoredProcedure<T>(string queryName, params object[] parameters);
        T ExecuteScalarStoredProcedure<T>(string queryName);
	
   
        T SaveOrUpdate<T>(T entity);
       
        T Insert<T>(T entity);
        void SubmitChanges();
        void Delete<T>(T entity) where T : IEntity;
    }
}
