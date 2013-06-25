﻿using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renfield.SafeRedir.Controllers;
using Renfield.SafeRedir.Models;
using Renfield.SafeRedir.Services;

namespace Renfield.SafeRedir.Tests.Controllers
{
  [TestClass]
  public class HomeControllerTests
  {
    [TestClass]
    public class Index : HomeControllerTests
    {
      [TestMethod]
      public void GetReturnsDefaultValues()
      {
        var svc = new Mock<ShorteningService>();
        var sut = new HomeController(svc.Object);

        var result = sut.Index() as ViewResult;

        var model = result.Model as RedirectInfo;
        Assert.IsNotNull(model);
        Assert.AreEqual("", model.URL);
        Assert.AreEqual("http://www.randomkittengenerator.com/", model.SafeURL);
        Assert.AreEqual(5 * 60, model.TTL);
      }

      [TestMethod]
      public void PostReturnsShortenedUrl()
      {
        var svc = new Mock<ShorteningService>();
        svc
          .Setup(it => it.CreateRedirect("http://example.com/", It.IsAny<string>(), It.IsAny<int>()))
          .Returns("123");
        var info = new RedirectInfo { URL = "example.com" };
        var sut = new HomeController(svc.Object);
        var helper = new MvcHelper();
        helper.SetUpController(sut);
        helper.Response.Setup(x => x.ApplyAppPathModifier("/r/123")).Returns("http://localhost/r/123");

        var result = (sut.Index(info) as ContentResult).Content;

        Assert.IsTrue(result.EndsWith("/r/123"), string.Format("Result is [{0}]", result));
      }

      [TestMethod]
      public void PostNormalizesGivenUrl()
      {
        var svc = new Mock<ShorteningService>();
        var info = new RedirectInfo { URL = "example.com" };
        var sut = new HomeController(svc.Object);
        var helper = new MvcHelper();
        helper.SetUpController(sut);

        sut.Index(info);

        svc.Verify(it => it.CreateRedirect("http://example.com/", It.IsAny<string>(), It.IsAny<int>()));
      }

      [TestMethod]
      public void PostNormalizesSafeUrl()
      {
        var svc = new Mock<ShorteningService>();
        var info = new RedirectInfo { URL = "example.com", SafeURL = "example.com" };
        var sut = new HomeController(svc.Object);
        var helper = new MvcHelper();
        helper.SetUpController(sut);

        sut.Index(info);

        svc.Verify(it => it.CreateRedirect(It.IsAny<string>(), "http://example.com/", It.IsAny<int>()));
      }

      [TestMethod]
      public void PostReturnsValidationErrorIfUrlIsMissing()
      {
        var svc = new Mock<ShorteningService>();
        var info = new RedirectInfo();
        var sut = new HomeController(svc.Object);
        var helper = new MvcHelper();
        helper.SetUpController(sut);
        sut.ValidateModel(info);

        sut.Index(info);

        Assert.IsFalse(sut.ModelState.IsValid);
        Assert.AreEqual(1, sut.ModelState["URL"].Errors.Count);
        Assert.AreEqual("Please enter the URL.", sut.ModelState["URL"].Errors[0].ErrorMessage);
      }
    }

    [TestClass]
    public class r : HomeControllerTests
    {
      [TestMethod]
      public void ReturnsRedirectFromService()
      {
        var svc = new Mock<ShorteningService>();
        var redirect = new RedirectResult("http://example.com");
        svc
          .Setup(it => it.GetUrl("abc"))
          .Returns(redirect);
        var sut = new HomeController(svc.Object);

        var result = sut.r("abc") as RedirectResult;

        Assert.AreEqual(redirect, result);
      }

      [TestMethod]
      public void Returns404ForUnknownId()
      {
        var svc = new Mock<ShorteningService>();
        var sut = new HomeController(svc.Object);

        var result = sut.r("abc") as HttpNotFoundResult;

        Assert.IsNotNull(result);
        Assert.AreEqual("Unknown id abc", result.StatusDescription);
      }
    }
  }
}