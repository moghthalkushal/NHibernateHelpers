# NHibernateHelpers
NHibernateHelpers with Structure Map

Just add the refrences of Core ,Infrastructure and DependanyResolver in to your project

Resolve the dependancy in your project using this code

 
        private static ILog _log = LogManager.GetLogger(typeof(IoCConfig));
        public static void Initialize() {
            IoC Ioc = new IoC();
            Ioc.Initialize();
            try {
                DependencyResolver.SetResolver(new StructureMapDependencyResolver(ObjectFactory.Container));
                GlobalConfiguration.Configuration.DependencyResolver = new StructureMapDependencyResolver(ObjectFactory.Container);
                ;
            }
            catch (Exception ex) { _log.Error(ex.Message, ex); throw; }
        }
    
    Inject them into your controller and start using the Nhibernate
       private IRepository<SqlServer> _sqlRepository;
        public FilterManagementController(IRepository<SqlServer> sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        
          var data = _sqlRepository.Execute_storeProc_or_queryFrom_xml<FilterResults>("CheckForNewFilters",
                new Dictionary<string, object> {
                                        { "TimeStamp", LastUpdatedTime},
                                        {"AllFilters_Required",AllFiltersRequired} }
                                        ).ToList();
                                        
           var item = _sqlRepository.Find<UserpreferencesForFilters>(x => x.UserPreferenceID == id).FirstOrDefault();                                        
