using NHibernateHelper.Core.Domain.Interfaces;

namespace NHibernateHelper.Infrastructure.DataAccess.Impl {
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IRepository<T> : IRepository where T : DatabaseEngine {
	}
}