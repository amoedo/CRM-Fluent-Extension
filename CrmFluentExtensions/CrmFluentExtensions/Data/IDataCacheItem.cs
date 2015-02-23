using System;
namespace CrmFluentExtensions.Data
{
    interface IDataCacheItem
    {
        DateTime Expiration { get; set; }
        bool IsValid { get; }
        object Value { get; set; }
    }
}
