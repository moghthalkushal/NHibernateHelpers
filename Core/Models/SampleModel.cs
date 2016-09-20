using System;
using System.Text;
using System.Collections.Generic;


namespace Sample.CustomerService.Domain {
    
    public class Filters {
        public virtual int FilterId { get; set; }
      
        public virtual string FilterName { get; set; }
        public virtual DateTime? CreatedOn { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual DateTime? ModifiedOn { get; set; }
        public virtual string ModifiedBy { get; set; }
    }
}
