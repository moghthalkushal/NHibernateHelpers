using NHibernateHelper.Core.Domain.Interfaces;
using NHibernateHelper.Core.Interfaces;

namespace NHibernateHelper.Core.Domain.Models {
    public abstract class PersistanceAssistedEntity<T> : IEntity<T> where T : IDatabase {
        private bool _isPersisted = false;
        public virtual bool IsPersisted { get { return _isPersisted; } }
        public virtual void SetAsPersistent() { _isPersisted = true; }
    }

}