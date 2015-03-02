
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmFluentExtensions.Examples
{
    class Program
    {
        static void Main(string[] args)
        {

            var mockService = PrepareMock();
            
            var fluentService = new FluentOrganizationService(mockService);

            var contact = new Entity("contact");

            Guid resultGuid;

            // Create a contact with 1 retry
            resultGuid = fluentService.CreateFluent(contact).Retry().Do();

            // Create a contact with up to 3 retries on errors and 10s space between 
            resultGuid = fluentService.CreateFluent(contact).Retry(10000, 3).Do();

            // Create a contact with up to 3 retries on errors and 10s space between 
            resultGuid = fluentService.CreateFluent(contact)
                .Retry(10000, 3,
                (ex) =>
                {
                    //Lambda Expression to manage the exceptions
                    Console.WriteLine("Exception on create {0}", ex.Message);
                },
                () =>
                {
                    //No more retries left and operation has not succeeded
                    Console.WriteLine("Not possible to create contact");
                    return Guid.Empty;
                })
                .Do(); //Executes the op

            // Create a contact with retries and logging 
            resultGuid = fluentService.CreateFluent(contact)
                .Retry(10000, 3)
                .Log((message) => Console.WriteLine(message), "About to start creation", "Creation Completed")
                .Do();

            resultGuid = fluentService.CreateFluent(contact)
                .Retry(10000, 3)
                .Log((message) => Console.WriteLine(message), "About to start creation", "Creation Completed")
                .HowLong((message)=> Console.WriteLine(message),"Starting timer", "It took {0}")
                .Do();

            //Create 50 Contacts
            int count = 0;
            resultGuid = fluentService.CreateFluent(contact)
                .Log((message) => Console.Write(message), "Creating 50 Contacts: ", "\nDone.")
                .While(
                () => { return count++ <= 50; },
                (newGuid) => { Console.Write("#"); })
                .Retry(10000, 2)
                .Delay(100)
                .Do();

            //Create 10 Contacts with different names
            count = 0;
            contact["firstname"] = "Contact 0";
            resultGuid = fluentService.CreateFluent(contact)
                .Log((message) => Console.WriteLine(message), "Creating 50 Contacts: ", "Done.")
                .While(
                () => { 
                    return count++ <= 10; 
                },
                (newGuid) => 
                { 
                    Console.WriteLine("Created {0} => {1}",contact["firstname"], newGuid); 
                    contact["firstname"] = string.Format("Contact {0}", count); 
                })
                .Log((message) => Console.Write(message), "<", ">")
                .Retry(10000, 2)
                .Delay(250)
                .Do();
            
            //Retrieve Multiple Take first
            Entity result = fluentService.RetrieveMultipleFluent(new QueryExpression())
                .FirstOrDefault()
                .Do();

            Console.WriteLine("Retrieved Contact with name  = {0}", result["firstname"]);
            Console.ReadLine();
        }

        private static IOrganizationService PrepareMock()
        {
            var mock = new Mock<IOrganizationService>();

            Entity expected = new Entity("contact");
            expected["firstname"] = "result";
            EntityCollection collection = new EntityCollection();
            collection.Entities.Add(expected);
            mock.Setup(s => s.RetrieveMultiple(It.IsAny<QueryBase>())).Returns(collection);

            var mockService = mock.Object;
            return mockService;
        }
    }
}
