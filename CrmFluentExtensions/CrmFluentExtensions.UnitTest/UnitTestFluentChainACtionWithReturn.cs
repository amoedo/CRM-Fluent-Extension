using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using CrmFluentExtensions;
using Microsoft.Xrm.Sdk.Query;


namespace CrmFluentExtensions.UnitTest
{
    [TestClass]
    public class UnitTestFluentChainACtionWithReturn
    {
        [TestMethod]
        public void TestRetry()
        {
            //Arrange
            var mockService = new Mock<IOrganizationService>();
            mockService.Setup(s => s.Create(It.IsAny<Entity>())).Throws(new InvalidOperationException());

            var service = new FluentOrganizationService(mockService.Object);

            var contact = new Entity("contact");

            //Act
            Guid result = service.CreateFluent(contact).Retry().Do();

            //Assert
            //Called twice as per default Retry
            mockService.Verify(s => s.Create(contact), Times.Exactly(2));
        }

        [TestMethod]
        public void TestRetryCount()
        {
            //Arrange
            var mockService = new Mock<IOrganizationService>();
            mockService.Setup(s => s.Create(It.IsAny<Entity>())).Throws(new InvalidOperationException());

            var service = new FluentOrganizationService(mockService.Object);

            var contact = new Entity("contact");

            int count = 0;

            Guid expected = Guid.NewGuid();

            //Act
            Guid result = service.CreateFluent(contact)
                .Retry(1, 2, //2 Retries 1ms apart
                (e) =>
                {
                    if (e is InvalidOperationException) count++;
                },
                () =>
                {
                    return expected;
                })
                .Do();

            //Assert
            //Called 3 times, exception managed 3 times and result is as expected
            mockService.Verify(s => s.Create(contact), Times.Exactly(3));
            Assert.AreEqual(count, 3);
            Assert.AreEqual(result, expected);
        }

        [TestMethod]
        public void TestUntil()
        {
            //Arrange
            Guid expected = Guid.NewGuid();

            var mockService = new Mock<IOrganizationService>();

            mockService.Setup(s => s.Create(It.IsAny<Entity>())).Returns(expected);

            var service = new FluentOrganizationService(mockService.Object);

            var contact = new Entity("contact");

            int count = 0;


            //Act
            Guid result = service.CreateFluent(contact)
                .Until(() =>
                    {
                        return ++count > 3;
                    })
                .Do();

            //Assert
            //Called 1 times, result is as expected and test evaluated until count > 3
            mockService.Verify(s => s.Create(contact), Times.Exactly(1));
            Assert.AreEqual(4, count);
            Assert.AreEqual(result, expected);
        }

        [TestMethod]
        public void TestWhile()
        {
            //Arrange
            Guid expected = Guid.Empty;

            var mockService = new Mock<IOrganizationService>();

            mockService.Setup(s => s.Create(It.IsAny<Entity>())).Returns(() => { expected = Guid.NewGuid(); return expected; });

            var service = new FluentOrganizationService(mockService.Object);

            var contact = new Entity("contact");

            int count = 0;


            //Act
            Guid result = service.CreateFluent(contact)
                .While(() =>
                {
                    return ++count <= 3;
                },
                (newValue) =>
                {
                    Assert.AreEqual(expected, newValue); //Check we are getting the results from the service
                }
                )
                .Do();

            //Assert
            //Called 3 times, result is as expected and test evaluated 
            mockService.Verify(s => s.Create(contact), Times.Exactly(3));
            Assert.AreEqual(4, count);
            Assert.AreEqual(result, expected); //Check we get the last value
        }

        [TestMethod]
        public void TestWhenTrue()
        {
            //Arrange
            Guid expected = Guid.NewGuid();

            var mockService = new Mock<IOrganizationService>();

            mockService.Setup(s => s.Create(It.IsAny<Entity>())).Returns(expected);

            var service = new FluentOrganizationService(mockService.Object);

            var contact = new Entity("contact");

            Guid result = Guid.Empty;

            //Act
            try
            {

                result = service.CreateFluent(contact)
                .WhenTrue(() =>
                {
                    return true;
                })
                .Do();

                 result = service.CreateFluent(contact)
                .WhenTrue(() =>
                {
                    return false;
                })
                .Do();

            }
            catch (Exception e)
            {
                //Expect exception on the second call
                Assert.IsInstanceOfType(e, typeof(OperationCanceledException));
            }


            //Assert
            //Called 1 times, result is as expected 
            mockService.Verify(s => s.Create(contact), Times.Once());
            Assert.AreEqual(expected, result);

        }

        //Extensions Test
        [TestMethod]
        public void TestFirst()
        {
            //Arrange
            
            var mockService = new Mock<IOrganizationService>();

            Entity expected = new Entity("contact");
            EntityCollection collection = new EntityCollection();
            collection.Entities.Add(expected);

            mockService.Setup(s => s.RetrieveMultiple(It.IsAny<QueryBase>())).Returns(collection);

            var service = new FluentOrganizationService(mockService.Object);

            
            //Act
            var result = service.RetrieveMultipleFluent(new QueryExpression())
                .First()
                .Do();

            //Assert
            //Called 3 times, exception managed 3 times and result is as expected
            mockService.Verify(s => s.RetrieveMultiple(It.IsAny<QueryBase>()), Times.Exactly(1));            
            Assert.AreEqual(result, expected);
        }

        [TestMethod]
        public void TestFirstWithEmpty()
        {
            //Arrange

            var mockService = new Mock<IOrganizationService>();

            Entity expected = new Entity("contact");
            EntityCollection collection = new EntityCollection();            

            mockService.Setup(s => s.RetrieveMultiple(It.IsAny<QueryBase>())).Returns(collection);

            var service = new FluentOrganizationService(mockService.Object);


            //Act
            try
            {
                var result = service.RetrieveMultipleFluent(new QueryExpression())
                    .First()
                    .Do();
                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentOutOfRangeException)
            {
               
            }

            //Assert
            //Called 3 times, exception managed 3 times and result is as expected
            mockService.Verify(s => s.RetrieveMultiple(It.IsAny<QueryBase>()), Times.Exactly(1));
        }

        [TestMethod]
        public void TestFirstOrDefault()
        {
            //Arrange

            var mockService = new Mock<IOrganizationService>();

            Entity expected = new Entity("contact");
            EntityCollection collection = new EntityCollection();
            collection.Entities.Add(expected);

            mockService.Setup(s => s.RetrieveMultiple(It.IsAny<QueryBase>())).Returns(collection);

            var service = new FluentOrganizationService(mockService.Object);


            //Act
            var result = service.RetrieveMultipleFluent(new QueryExpression())
                .FirstOrDefault()
                .Do();

            //Assert
            //Called 3 times, exception managed 3 times and result is as expected
            mockService.Verify(s => s.RetrieveMultiple(It.IsAny<QueryBase>()), Times.Exactly(1));
            Assert.AreEqual(result, expected);
        }


        [TestMethod]
        public void TestFirstOrDefaultWithEmpty()
        {
            //Arrange

            var mockService = new Mock<IOrganizationService>();

            Entity expected = new Entity("contact");
            EntityCollection collection = new EntityCollection();

            mockService.Setup(s => s.RetrieveMultiple(It.IsAny<QueryBase>())).Returns(collection);

            var service = new FluentOrganizationService(mockService.Object);


            //Act

                var result = service.RetrieveMultipleFluent(new QueryExpression())
                    .FirstOrDefault()
                    .Do();


            //Assert
            //Called 3 times, exception managed 3 times and result is as expected
            mockService.Verify(s => s.RetrieveMultiple(It.IsAny<QueryBase>()), Times.Exactly(1));
            Assert.AreEqual(default(Entity), result);
        }

    }
}
