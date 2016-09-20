using System;
using System.Linq;
using FluentNHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Mapping;
using FluentNHibernate.Mapping.Providers;

namespace NHibernateHelper.Infrastructure.DataAccess.Extensions {
    public static class FluentExtensions {
        public static FluentMappingsContainer AddFromAssemblyOf<T>(this FluentMappingsContainer mappings, Predicate<Type> where) {
            if (where == null)
                return mappings.AddFromAssemblyOf<T>();

            var mappingClasses = typeof(T).Assembly.GetExportedTypes()
                .Where(x => (typeof(IMappingProvider).IsAssignableFrom(x) || typeof(IIndeterminateSubclassMappingProvider).IsAssignableFrom(x)))
                .Where(x => where(x));

            foreach (var type in mappingClasses) {
                mappings.Add(type);
            }

            return mappings;
        }

        public static PropertyPart AsDateTime2(this PropertyPart map) {
            return map.CustomType("datetime2");
        }

        public static PropertyPart AsDate(this PropertyPart map) {
            return map.CustomType("date");
        }

        public static PropertyPart AsYesNoBoolean(this PropertyPart map) {
            return map.CustomType("YesNo");
        }
    }
}
