
using NHibernateHelper.Infrastructure.DataAccess;
using NHibernateHelper.Infrastructure.DataAccess.Impl;

using NHibernate;

using StructureMap.Configuration.DSL;


namespace NHibernateHelper.Infrastructure.DependencyResolution
{

    public class SqlServerRegistry : Registry
    {
        public SqlServerRegistry()
        {
            string sql = typeof(SqlServer).ToString();

            For<IRepository<SqlServer>>().HybridHttpOrThreadLocalScoped().Use(ctx => {
                return new Repository<SqlServer>(ctx.GetInstance<ISessionFactory>(sql));
            });

            For<ISessionFactory>().Singleton()
                .Use(() => NHibernateHelpers.BuildSqlServerSessionFactory()).Named(sql);
        }
    }
}
