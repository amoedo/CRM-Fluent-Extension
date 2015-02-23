using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmFluentExtensions
{
    /// <summary>
    /// Oganization Service that implements a fluent interface
    /// The IOrganizationService methods with Fluent suffix
    /// allow creating a chain of fluent wrappers to perform
    /// several chained operations before or after the call to CRM is done.
    /// </summary>
    public class FluentOrganizationService : IOrganizationService
    {

        IOrganizationService service;

        public FluentOrganizationService(IOrganizationService service)
        {
            this.service = service;
        }

        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            this.service.Associate(entityName, entityId, relationship, relatedEntities);
        }

        public Guid Create(Entity entity)
        {
            return service.Create(entity);
        }

        public FluentChainActionWithReturn<Guid> CreateFluent(Entity entity)
        {
            return new FluentChainActionWithReturn<Guid>(() =>
            {
                return service.Create(entity);
            });
        }

        public void Delete(string entityName, Guid id)
        {
            service.Delete(entityName, id);
        }

        public FluentChainAction DeleteFluent(string entityName, Guid id)
        {
            return new FluentChainAction(() =>
            {
                service.Delete(entityName, id);
            });
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            service.Disassociate(entityName, entityId, relationship, relatedEntities);
        }

        public FluentChainAction DisassociateFluent(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            return new FluentChainAction(() =>
            {
                service.Disassociate(entityName, entityId, relationship, relatedEntities);
            });
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            return service.Execute(request);
        }

        public FluentChainActionWithReturn<OrganizationResponse> ExecuteFluent(OrganizationRequest request)
        {
            return new FluentChainActionWithReturn<OrganizationResponse>(() =>
            {
                return service.Execute(request);
            });
        }

        public Entity Retrieve(string entityName, Guid id, Microsoft.Xrm.Sdk.Query.ColumnSet columnSet)
        {
            return service.Retrieve(entityName, id, columnSet);
        }

        public FluentChainActionWithReturn<Entity> RetrieveFluent(string entityName, Guid id, Microsoft.Xrm.Sdk.Query.ColumnSet columnSet)
        {
            return new FluentChainActionWithReturn<Entity>(() =>
            {
                return service.Retrieve(entityName, id, columnSet);
            });

        }

        public EntityCollection RetrieveMultiple(Microsoft.Xrm.Sdk.Query.QueryBase query)
        {
            return service.RetrieveMultiple(query);
        }

        public FluentChainActionWithReturn<EntityCollection> RetrieveMultipleFluent(Microsoft.Xrm.Sdk.Query.QueryBase query)
        {
            return new FluentChainActionWithReturn<EntityCollection>(() =>
            {
                return service.RetrieveMultiple(query);
            });
        }

        public void Update(Entity entity)
        {
            service.Update(entity);
        }

        public FluentChainAction UpdateFluent(Entity entity)
        {
            return new FluentChainAction(() =>
            {
                service.Update(entity);
            });
        }
    }

}
