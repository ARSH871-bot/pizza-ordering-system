using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Models;

namespace PizzaExpress.Tests
{
    [TestClass]
    public class CustomerTests
    {
        [TestMethod]
        public void FullName_ReturnsCombinedFirstAndLastName()
        {
            var c = new Customer { FirstName = "Jane", LastName = "Doe" };
            Assert.AreEqual("Jane Doe", c.FullName);
        }

        [TestMethod]
        public void FullName_NoLastName_ReturnsFirstNameOnly()
        {
            var c = new Customer { FirstName = "Jane", LastName = null };
            Assert.AreEqual("Jane", c.FullName);
        }

        [TestMethod]
        public void FullName_NoFirstName_ReturnsEmpty()
        {
            var c = new Customer { FirstName = null, LastName = null };
            Assert.AreEqual(string.Empty, c.FullName);
        }

        [TestMethod]
        public void AllFields_CanBeSetAndRetrieved()
        {
            var c = new Customer
            {
                FirstName  = "John",
                LastName   = "Smith",
                Address    = "1 Queen St",
                City       = "Auckland",
                Region     = "Auckland",
                PostalCode = "1010",
                ContactNo  = "0211234567",
                Email      = "john@example.com",
            };

            Assert.AreEqual("John",             c.FirstName);
            Assert.AreEqual("Smith",            c.LastName);
            Assert.AreEqual("1 Queen St",       c.Address);
            Assert.AreEqual("Auckland",         c.City);
            Assert.AreEqual("Auckland",         c.Region);
            Assert.AreEqual("1010",             c.PostalCode);
            Assert.AreEqual("0211234567",       c.ContactNo);
            Assert.AreEqual("john@example.com", c.Email);
        }
    }
}
