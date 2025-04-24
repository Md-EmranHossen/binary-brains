using System;
using System.Collections.Generic;
using AmarTech.Web.Areas.Identity.Pages.Account.Manage;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace ECommerceSystem.Test
{
    public class ManageNavPagesTests
    {
        #region Static Property Tests

        [Fact]
        public void StaticProperties_ReturnExpectedValues()
        {
            // Verify that all static string properties return the expected values
            Assert.Equal("Index", ManageNavPages.Index);
            Assert.Equal("Email", ManageNavPages.Email);
            Assert.Equal("ChangePassword", ManageNavPages.ChangePassword);
            Assert.Equal("DownloadPersonalData", ManageNavPages.DownloadPersonalData);
            Assert.Equal("DeletePersonalData", ManageNavPages.DeletePersonalData);
            Assert.Equal("ExternalLogins", ManageNavPages.ExternalLogins);
            Assert.Equal("PersonalData", ManageNavPages.PersonalData);
            Assert.Equal("TwoFactorAuthentication", ManageNavPages.TwoFactorAuthentication);
        }

        #endregion

        #region NavClass Method Tests

        [Theory]
        [InlineData("Index")]
        [InlineData("Email")]
        [InlineData("ChangePassword")]
        [InlineData("DownloadPersonalData")]
        [InlineData("DeletePersonalData")]
        [InlineData("ExternalLogins")]
        [InlineData("PersonalData")]
        [InlineData("TwoFactorAuthentication")]
        public void SpecificNavClass_WhenActivePage_ReturnsActiveClass(string pageName)
        {
            // Arrange
            var viewContext = CreateViewContext(pageName);

            // Act & Assert - Test the appropriate method based on the page name
            switch (pageName)
            {
                case "Index":
                    Assert.Equal("active", ManageNavPages.IndexNavClass(viewContext));
                    break;
                case "Email":
                    Assert.Equal("active", ManageNavPages.EmailNavClass(viewContext));
                    break;
                case "ChangePassword":
                    Assert.Equal("active", ManageNavPages.ChangePasswordNavClass(viewContext));
                    break;
                case "DownloadPersonalData":
                    Assert.Equal("active", ManageNavPages.DownloadPersonalDataNavClass(viewContext));
                    break;
                case "DeletePersonalData":
                    Assert.Equal("active", ManageNavPages.DeletePersonalDataNavClass(viewContext));
                    break;
                case "ExternalLogins":
                    Assert.Equal("active", ManageNavPages.ExternalLoginsNavClass(viewContext));
                    break;
                case "PersonalData":
                    Assert.Equal("active", ManageNavPages.PersonalDataNavClass(viewContext));
                    break;
                case "TwoFactorAuthentication":
                    Assert.Equal("active", ManageNavPages.TwoFactorAuthenticationNavClass(viewContext));
                    break;
            }
        }

        [Theory]
        [InlineData("Index", "Email")]
        [InlineData("Email", "Index")]
        [InlineData("ChangePassword", "Index")]
        [InlineData("DownloadPersonalData", "Index")]
        [InlineData("DeletePersonalData", "Index")]
        [InlineData("ExternalLogins", "Index")]
        [InlineData("PersonalData", "Index")]
        [InlineData("TwoFactorAuthentication", "Index")]
        public void SpecificNavClass_WhenNotActivePage_ReturnsNull(string activePage, string requestedPage)
        {
            // Arrange
            var viewContext = CreateViewContext(activePage);

            // Act & Assert - Test the appropriate method based on the requested page
            switch (requestedPage)
            {
                case "Index":
                    Assert.Null(ManageNavPages.IndexNavClass(viewContext));
                    break;
                case "Email":
                    Assert.Null(ManageNavPages.EmailNavClass(viewContext));
                    break;
                case "ChangePassword":
                    Assert.Null(ManageNavPages.ChangePasswordNavClass(viewContext));
                    break;
                case "DownloadPersonalData":
                    Assert.Null(ManageNavPages.DownloadPersonalDataNavClass(viewContext));
                    break;
                case "DeletePersonalData":
                    Assert.Null(ManageNavPages.DeletePersonalDataNavClass(viewContext));
                    break;
                case "ExternalLogins":
                    Assert.Null(ManageNavPages.ExternalLoginsNavClass(viewContext));
                    break;
                case "PersonalData":
                    Assert.Null(ManageNavPages.PersonalDataNavClass(viewContext));
                    break;
                case "TwoFactorAuthentication":
                    Assert.Null(ManageNavPages.TwoFactorAuthenticationNavClass(viewContext));
                    break;
            }
        }

        #endregion

        #region PageNavClass Tests

        [Fact]
        public void PageNavClass_WithMatchingActivePage_ReturnsActiveClass()
        {
            // Arrange
            var viewContext = CreateViewContext("TestPage");

            // Act
            var result = ManageNavPages.PageNavClass(viewContext, "TestPage");

            // Assert
            Assert.Equal("active", result);
        }

        [Fact]
        public void PageNavClass_WithNonMatchingActivePage_ReturnsNull()
        {
            // Arrange
            var viewContext = CreateViewContext("OtherPage");

            // Act
            var result = ManageNavPages.PageNavClass(viewContext, "TestPage");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void PageNavClass_WithCaseDifference_ReturnsActiveClass()
        {
            // Arrange
            var viewContext = CreateViewContext("testpage");

            // Act
            var result = ManageNavPages.PageNavClass(viewContext, "TestPage");

            // Assert
            Assert.Equal("active", result);
        }

        [Fact]
        public void PageNavClass_WithoutActivePageInViewData_UsesActionDescriptorDisplayName()
        {
            // Arrange
            var viewContext = new ViewContext
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
                ActionDescriptor = new ActionDescriptor
                {
                    DisplayName = "TestPage.cshtml"
                }
            };

            // Act
            var result = ManageNavPages.PageNavClass(viewContext, "TestPage");

            // Assert
            Assert.Equal("active", result);
        }

        [Fact]
        public void PageNavClass_WithActionDescriptorPath_ExtractsFileNameWithoutExtension()
        {
            // Arrange
            var viewContext = new ViewContext
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
                ActionDescriptor = new ActionDescriptor
                {
                    DisplayName = "/Path/To/TestPage.cshtml"
                }
            };

            // Act
            var result = ManageNavPages.PageNavClass(viewContext, "TestPage");

            // Assert
            Assert.Equal("active", result);
        }

        #endregion

        #region Helper Methods

        private static ViewContext CreateViewContext(string activePage)
        {
            return new ViewContext
            {
                ViewData = new ViewDataDictionary(
                    new EmptyModelMetadataProvider(),
                    new ModelStateDictionary())
                {
                    ["ActivePage"] = activePage
                }
            };
        }

        #endregion
    }
}