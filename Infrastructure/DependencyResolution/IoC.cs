using System;
using log4net;
using StructureMap;

namespace NHibernateHelper.Infrastructure.DependencyResolution {
	/// <summary>
	/// 
	/// </summary>
	public  class IoC {
		private static ILog _log = LogManager.GetLogger(typeof(IoC));
		
		/// <summary>
		/// 
		/// </summary>
		public  void Initialize() {
			try {
				var namespaceRoot = "NHibernateHelper";
              
                
               
                ObjectFactory.Initialize(x => {
					x.Scan(y => {
						y.AssembliesFromApplicationBaseDirectory(assembly =>
							assembly.GetName().Name.Contains(namespaceRoot));
						y.LookForRegistries();
					});
				});
			}
			catch (Exception ex) { _log.Error(ex.Message, ex); throw; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T Resolve<T>() {
			return ObjectFactory.GetInstance<T>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public static T Resolve<T>(string name) {
			return ObjectFactory.GetNamedInstance<T>(name);
		}
	}
}