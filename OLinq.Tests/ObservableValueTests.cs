using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{
    [TestClass]
    public class ObservableValueTests
    {
        [TestMethod]
        public void NotYetImplemented_CanObserveAppendedStrings()
        {
            var person = new Person("Mary", "Smith", DateTimeOffset.Parse("1 Jan 2000"));
            var fullname = new ObservableValue<string>(() => person.FirstName + person.LastName);
            person.LastName = "Jones";
            Assert.AreEqual("Mary Jones", fullname.Value);
        }

        [TestMethod]
        public void CanObserveCalculatedNumber()
        {
            var person = new Person("Mary", "Smith", DateTimeOffset.Parse("1 Jan 2000"));
            var yearBorn = new ObservableValue<int>(() => person.Dob.Year);
            person.Dob = DateTimeOffset.Parse("1 Jan 2001");
            Assert.AreEqual(2001, yearBorn.Value);
        }

        [TestMethod]
        public void RaiseExtensionTest()
        {
            var person = new Person("Mary", "Smith", DateTimeOffset.Parse("1 Jan 2000"));
            var yearOfBirthChanged = false;
            person.PropertyChanged += (s, a) => yearOfBirthChanged |= ("YearOfBirth" == a.PropertyName);
            
            person.Dob = DateTimeOffset.Parse("1 Jan 2001");
            
            Assert.AreEqual(2001, person.YearOfBirth);
            Assert.IsTrue(yearOfBirthChanged);

        }
    }
}