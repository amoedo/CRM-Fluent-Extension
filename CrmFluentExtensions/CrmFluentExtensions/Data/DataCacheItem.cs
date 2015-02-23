using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmFluentExtensions.Data
{
    public class DataCacheItem : IDataCacheItem
    {
        public DateTime Expiration { get; set; }
        public object Value { get; set; }
        public bool IsValid
        {
            get
            {
                return (Value != null) && (Expiration > DateTime.Now);
            }
        }
    }
}
