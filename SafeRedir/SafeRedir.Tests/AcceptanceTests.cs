﻿using System.Linq;
using System.Net;
using System.Threading;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renfield.SafeRedir.Tests
{
  [TestClass]
  public class AcceptanceTests
  {
    private const string BASE_URL = "http://localhost:8447";
    private const string URL = BASE_URL + "/";
    private const string REDIRECT_PREFIX = BASE_URL + "/r/";

    [TestMethod]
    public void GetIndexReturnsFormWithDefaultValues()
    {
      var root = LoadHtml("/");

      var form = root.SelectSingleNode("//form");
      Assert.IsNotNull(form);
      var url = form.SelectSingleNode("//input[@id='URL']");
      Assert.IsNotNull(url);
      Assert.AreEqual("", url.Attributes["Value"].Value);
      var safeUrl = form.SelectSingleNode("//input[@id='SafeURL']");
      Assert.IsNotNull(safeUrl);
      Assert.AreEqual("http://www.randomkittengenerator.com/", safeUrl.Attributes["Value"].Value);
      var expiresInMins = form.SelectSingleNode("//input[@id='TTL']");
      Assert.IsNotNull(expiresInMins);
      Assert.AreEqual("300", expiresInMins.Attributes["Value"].Value);
      var submit = form.SelectSingleNode("//button[@type='submit']");
      Assert.IsNotNull(submit);
    }

    [TestMethod]
    public void PostIndexReturnsNewUrlRedirectingToOriginal()
    {
      using (var web = new WebClient())
      {
        var data = string.Format("URL=example.com&SafeURL={0}&TTL={1}", "http://www.randomkittengenerator.com/", 300);
        web.Headers["Content-Type"] = "application/x-www-form-urlencoded";
        var shortened = web.UploadString(URL, data);
        Assert.IsTrue(shortened.StartsWith(REDIRECT_PREFIX));

        var res = SafeGet(shortened);
        Assert.AreEqual("Redirect", res.StatusCode.ToString());
        Assert.AreEqual("http://example.com/", res.Headers["Location"]);
      }
    }

    [TestMethod]
    public void PostIndexReturnsNewUrlRedirectingToSafeAfterTTL()
    {
      using (var web = new WebClient())
      {
        var data = string.Format("URL=example.com&SafeURL={0}&TTL={1}", "http://www.randomkittengenerator.com/", 1);
        web.Headers["Content-Type"] = "application/x-www-form-urlencoded";
        var shortened = web.UploadString(URL, data);
        Assert.IsTrue(shortened.StartsWith(REDIRECT_PREFIX));

        Thread.Sleep(1100); // sleep long enough for the TTL to expire

        var res = SafeGet(shortened);
        Assert.AreEqual("MovedPermanently", res.StatusCode.ToString());
        Assert.AreEqual("http://www.randomkittengenerator.com/", res.Headers["Location"]);
      }
    }

    [TestMethod]
    public void GetStatsReturnsNumberOfRecordsForSeveralPeriods()
    {
      var root = LoadHtml("/Home/Stats");

      var list = root.SelectSingleNode("//*[@id='stats']");
      var items = list.SelectNodes(".//li").ToList();

      Assert.AreEqual(4, items.Count);
      Assert.AreEqual("Today", items[0].SelectSingleNode(".//span[@class='period']").InnerText);
      Assert.IsNotNull(items[0].SelectSingleNode(".//span[@class='count']").InnerText);
      Assert.AreEqual("Current Month", items[1].SelectSingleNode(".//span[@class='period']").InnerText);
      Assert.IsNotNull(items[1].SelectSingleNode(".//span[@class='count']").InnerText);
      Assert.AreEqual("Current Year", items[2].SelectSingleNode(".//span[@class='period']").InnerText);
      Assert.IsNotNull(items[2].SelectSingleNode(".//span[@class='count']").InnerText);
      Assert.AreEqual("Overall", items[3].SelectSingleNode(".//span[@class='period']").InnerText);
      Assert.IsNotNull(items[3].SelectSingleNode(".//span[@class='count']").InnerText);
    }

    [TestMethod]
    public void GetDisplayWithoutKeyReturnsNotFound()
    {
      try
      {
        SafeGet(BASE_URL + "/Home/Display");
        Assert.Fail("Should have thrown.");
      }
      catch (WebException ex)
      {
        Assert.AreEqual("NotFound", ((HttpWebResponse) ex.Response).StatusCode.ToString());
      }
    }

    [TestMethod]
    public void GetDisplayWithKeyReturnsFormWithTwoFieldsAndAButton()
    {
      var root = LoadHtml("/Home/Display/" + Constants.SECRET);

      var form = root.SelectSingleNode("//form");
      Assert.IsNotNull(form);
      var fromDate = form.SelectSingleNode("//input[@id='FromDate']");
      Assert.IsNotNull(fromDate);
      var toDate = form.SelectSingleNode("//input[@id='ToDate']");
      Assert.IsNotNull(toDate);
      var submit = form.SelectSingleNode("//button[@type='submit']");
      Assert.IsNotNull(submit);
    }

    [TestMethod]
    public void PostDisplayWithKeyReturnsTableWithLinks()
    {
      using (var web = new WebClient())
      {
        const string data = "FromDate=&ToDate=";
        web.Headers["Content-Type"] = "application/x-www-form-urlencoded";
        var html = web.UploadString(BASE_URL + "/Home/Display/" + Constants.SECRET, data);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var root = doc.DocumentNode;

        var table = root.SelectSingleNode("//table");
        var rows = table.SelectNodes(".//tr");
        var cells = table.SelectNodes(".//th|.//td");

        Assert.IsTrue(rows.Count > 0);
        Assert.IsTrue(cells.Count > 0);
      }
    }

    [TestMethod]
    public void Pagination()
    {
      using (var web = new WebClient())
      {
        const string data = "FromDate=&ToDate=";
        web.Headers["Content-Type"] = "application/x-www-form-urlencoded";
        var html = web.UploadString(BASE_URL + "/Home/Display/" + Constants.SECRET, data);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var root = doc.DocumentNode;

        var pages = root.SelectSingleNode("//div[@id='pages']");
        Assert.IsNotNull(pages);
        var pageNumbers = pages.SelectSingleNode(".//ol");
        Assert.IsNotNull(pageNumbers);
        var currentPage = pageNumbers.SelectSingleNode("li[@class='currentPage']");
        Assert.IsNotNull(currentPage);
        Assert.AreEqual("1", currentPage.InnerText);
      }
    }

    //

    private static HtmlNode LoadHtml(string page)
    {
      using (var web = new WebClient())
      {
        var html = web.DownloadString(BASE_URL + page);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        return doc.DocumentNode;
      }
    }

    private static HttpWebResponse SafeGet(string url)
    {
      var req = (HttpWebRequest) WebRequest.Create(url);
      req.AllowAutoRedirect = false;

      return (HttpWebResponse) req.GetResponse();
    }
  }
}