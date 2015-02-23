using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace CrmFluentExtensions.Data
{
    public static class DataCache
    {
        static private ConcurrentDictionary<string, IDataCacheItem> storage = new ConcurrentDictionary<string, IDataCacheItem>();

        public static TimeSpan DefaulTimeToLeave = new TimeSpan(0,0,60);
        
        public static bool HasValue(string key)
        {
            if (!storage.ContainsKey(key))
            {
                return false;
            }
            else 
            {
                return storage[key].IsValid;
            }
           
        }

        public static void SetValue(string key, object value, TimeSpan timeToLive) 
        {
            var item = new DataCacheItem()
            {
                Expiration = DateTime.Now.Add(timeToLive),
                Value = value
            };

            //TODO: Review
            storage.AddOrUpdate(key, item, (oldKey, oldValue) => { return item; });
        }

        public static void SetValue(string key, object value)
        {
            SetValue(key, value, DefaulTimeToLeave);
        }

        public static T GetValue<T>(string key)
        {
            if (HasValue(key))
            {
                return (T) (storage[key]).Value;
            }
            else
            {
                return default(T);
            }
        }


    }
}
