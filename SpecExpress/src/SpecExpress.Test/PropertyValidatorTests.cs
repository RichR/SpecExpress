﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SpecExpress.Rules.StringValidators;
using SpecExpress.Test.Entities;

namespace SpecExpress.Test
{
    [TestFixture]
    public class PropertyValidatorTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            ValidationCatalog.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            ValidationCatalog.Reset();
        }

        #endregion

        [Test]
        [Ignore]
        public void Validate_OptionalProperty_WithNoValue_IsValid()
        {
            var emptyContact = new Contact();
            emptyContact.FirstName = string.Empty;
            emptyContact.LastName = string.Empty;

            var propertyValidator =
                new PropertyValidator<Contact, string>(contact => contact.LastName);

            //add a single rule
            var lengthValidator = new LengthBetween<Contact>(1, 5);
            propertyValidator.AndRule(lengthValidator); //.Rules.Add(lengthValidator);

            var notification = new ValidationNotification();

            //Validate
            var result = propertyValidator.Validate(emptyContact, null, notification);

            Assert.That(result, Is.True);
            Assert.That(notification.Errors, Is.Empty);
        }

        [Test]
        public void Validate_OptionalNestedProperty_WithNullValue_IsValid()
        {
            var customer = new Customer();

            ValidationCatalog.AddSpecification<Customer>( spec => spec.Check(cust => cust.Address.Street).Optional()
                .MaxLength(255));

            var results = ValidationCatalog.Validate(customer);

            Assert.That(results.Errors, Is.Empty);

        }


        [Test]
        public void Validate_OptionalCollection_Using_Registered_Specification()
        {
            //Build test data
            var customer = new Customer() { Name = "TestCustomer"};
            var validContact = new Contact() {FirstName = "Johnny B", LastName = "Good"};
            var invalidContact = new Contact() { FirstName = "Baddy"};

            customer.Contacts = new List<Contact>() {validContact, invalidContact};

            //Build specifications
            ValidationCatalog.AddSpecification<Customer>(spec =>
                                                               {
                                                                   spec.Check(cust => cust.Name).Required();
                                                                   spec.Check(cust => cust.Contacts).Required();
                                                               });

            ValidationCatalog.AddSpecification<Contact>(spec =>
                                                              {
                                                                  spec.Check(c => c.FirstName).Required();
                                                                  spec.Check(c => c.LastName).Required();
                                                              });


            ValidationCatalog.ValidateObjectGraph = true;

            //Validate
            var results = ValidationCatalog.Validate(customer);

            Assert.That(results.Errors.Count, Is.AtLeast(1));

        }

        [Test]
        public void Validate_Property_With_PropertyNameOverrideExpression_IsValid()
        {
            var emptyContact = new Contact();
            emptyContact.FirstName = "George's last name";
            emptyContact.LastName = string.Empty;

            var propertyValidator =
                new PropertyValidator<Contact, string>(contact => contact.LastName);

            propertyValidator.PropertyNameOverrideExpression = new Func<Contact, string>( o => o.FirstName);

            //add a single rule
            var lengthValidator = new LengthBetween<Contact>(1, 5);
            propertyValidator.AndRule(lengthValidator);

            //Validate
            ValidationNotification notification = new ValidationNotification();
            propertyValidator.Validate(emptyContact, null, notification);

            Assert.That(notification.Errors, Is.Not.Empty);
        }

        [Test]
        public void Validate_Property_With_NullCondition_IsValid()
        {
            // String.IsNullOrEmpty(c.Addresses[0].City) should throw an exception
            ValidationCatalog.AddSpecification<Contact>(spec => spec.Check(c => c.FirstName).If( c=> String.IsNullOrEmpty(c.Addresses[0].City)).Required());
            var vn = ValidationCatalog.Validate(new Contact());
            Assert.That(vn.IsValid, Is.True);
        }

        [Test]
        public void Validate_Property_With_CustomName_IsValid_And_Label_Is_Set()
        {
            const string expected = "This is a custom rule label";

            ValidationCatalog.AddSpecification<Contact>(spec => 
                spec.Check(c => c.FirstName).WithLabel(expected)
                .If(c => String.IsNullOrEmpty(c.Addresses[0].City)).Required());

            var vn = ValidationCatalog.Validate(new Contact());
            Assert.That(vn.IsValid, Is.True);

            var actual = ValidationCatalog.SpecificationContainer.GetAllSpecifications().Single().PropertyValidators.Single().Label;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Validate_Property_Label_Is_Available_On_Error_When_Invalid()
        {
            const string expected = "This is a custom rule label";

            ValidationCatalog.AddSpecification<Contact>(spec =>
                spec.Check(c => c.FirstName).WithLabel(expected).Required());

            var contact = new Contact();
            var vn = ValidationCatalog.Validate(contact);
            Assert.That(vn.IsValid, Is.False);

            var error = vn.Errors.Single();
            Assert.AreEqual(expected, error.Label);
        }
    }
}