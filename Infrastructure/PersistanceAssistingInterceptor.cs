using NHibernateHelper.Core.Domain.Interfaces;
using NHibernateHelper.Core.Domain.Model;
using NHibernate;
using NHibernate.Type;

namespace NHibernateHelper.Infrastructure.DataAccess {
    public class PersistanceAssistingInterceptor<T> : EmptyInterceptor where T : IDatabase {
        public override bool? IsTransient(object entity) {
            if (entity is PersistanceAssistedEntity<T>) { return !((PersistanceAssistedEntity<T>)entity).IsPersisted; } else { return null; }
        }

        public override bool OnLoad(object entity, object id, object[] state, string[] propertyNames, IType[] types) {
            if (entity is PersistanceAssistedEntity<T>) ((PersistanceAssistedEntity<T>)entity).SetAsPersistent();
            return false;
        }

        public override bool OnSave(object entity, object id, object[] state, string[] propertyNames, IType[] types) {
            if (entity is PersistanceAssistedEntity<T>) ((PersistanceAssistedEntity<T>)entity).SetAsPersistent();
            return false;
        }
    }
}
