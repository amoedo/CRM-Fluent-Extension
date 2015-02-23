using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

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
                .Until(()=>
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
    }
}
