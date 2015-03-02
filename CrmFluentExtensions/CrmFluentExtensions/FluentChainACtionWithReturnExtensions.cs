using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmFluentExtensions
{
    public static class FluentChainACtionWithReturnExtensions
    {
        public static FluentChainActionWithReturn<Entity> First(this FluentChainActionWithReturn<EntityCollection> chain)
        {
            var result = new FluentChainActionWithReturn<Entity>(() =>
            {
                var collection = chain.Do();

                return collection.Entities[0];
            });

            return result;
        }

        public static FluentChainActionWithReturn<Entity> FirstOrDefault(this FluentChainActionWithReturn<EntityCollection> chain)
        {
            var result = new FluentChainActionWithReturn<Entity>(() =>
            {
                var collection = chain.Do();                

                if (collection.Entities != null && collection.Entities.Count > 0)
                    return collection.Entities[0];
                else
                    return default(Entity);
            });

            return result;
        }

    }
}
