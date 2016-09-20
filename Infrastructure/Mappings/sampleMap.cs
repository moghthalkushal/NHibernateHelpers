
using FluentNHibernate.Mapping;
using Sample.CustomerService.Domain;

namespace Sample.CustomerService.Maps
{


    public class FiltersMap : ClassMap<Filters>
    {
        //Here you map your Database table names with your class and its properties.
       // Make sure your properties are virtual 
       // Column names are CaseSensitive
        public FiltersMap()
        {
            Table("Filters");
            LazyLoad();
            Id(x => x.FilterId).GeneratedBy.Identity().Column("FilterId");
            Map(x => x.FilterName).Column("FilterName");
            Map(x => x.CreatedOn).Column("CreatedOn");
            Map(x => x.CreatedBy).Column("CreatedBy");
            Map(x => x.ModifiedOn).Column("ModifiedOn");
            Map(x => x.ModifiedBy).Column("ModifiedBy");
        }
    }
}
