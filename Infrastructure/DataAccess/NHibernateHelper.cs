using System;
using log4net;
using NHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernateHelper.Infrastructure.DataAccess.Impl;
using NHibernate.Cfg;
using NHibernateHelper.Core.Database;
using NHibernateHelper.Infrastructure.DataAccess;

namespace NHibernateHelper.Infrastructure.DataAccess
{
    internal static class NHibernateHelpers
    {
        private static ILog _log = LogManager.GetLogger(typeof(NHibernateHelpers));

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static ISessionFactory BuildSqlServerSessionFactory()
        {
            return Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2012
                        .ConnectionString(c => c.FromConnectionStringWithKey("sqlServerConnection"))
                        .Provider<SqlServerNoCountOffConnectionProvider>().ShowSql())
                .Mappings(mapping => {
                    mapping.FluentMappings.AddFromAssemblyOf<Repository<SqlServer>>();
                    //mapping.HbmMappings.AddFromAssemblyOf<Repository<SqlServer>>();
                })
                .ExposeConfiguration(c => c.DataBaseIntegration(prop => prop.BatchSize = 1000))
                .ExposeConfiguration(c => c.SetInterceptor(new PersistanceAssistingInterceptor<SQL>()))
                .BuildSessionFactory();
        }

        public static ISession OpenSessionExtension(this ISessionFactory sessionFactory)
        {
            try
            {
                var session = sessionFactory.OpenSession();
                session.FlushMode = FlushMode.Commit;
                session.BeginTransaction();
                return session;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionFactory"></param>
        /// <returns></returns>
        public static IStatelessSession OpenStatelessSessionExtension(this ISessionFactory sessionFactory)
        {
            try
            {
                var session = sessionFactory.OpenStatelessSession();
                session.BeginTransaction();
                return session;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }
    }
}