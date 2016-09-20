using NHibernateHelper.Core.Domain.Interfaces;

namespace NHibernateHelper.Core.Interfaces {
    public interface IEntity { }

    public interface IEntity<T> where T : IDatabase { }
}
