﻿using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renfield.SimpleViewEngine.Library;
using Renfield.SimpleViewEngine.Library.AST;

namespace Renfield.SimpleViewEngine.Tests
{
  [TestClass]
  public class ParserTests
  {
    [TestClass]
    public class Parse : ParserTests
    {
      [TestMethod]
      public void ReturnsEmptyEnumerable()
      {
        var sut = new Parser();

        var result = sut.Parse("").ToList();

        Assert.AreEqual(0, result.Count);
      }

      [TestMethod]
      public void ReturnsOneConstantNodeWithCorrectValue()
      {
        dynamic model = new ExpandoObject();
        var sut = new Parser();

        var result = sut.Parse("test").ToList();

        Assert.AreEqual(1, result.Count);
        Assert.IsInstanceOfType(result[0], typeof (ConstantNode));
        Assert.AreEqual("test", result[0].Eval(model));
      }

      [TestMethod]
      public void ReturnsOnePropertyNodeWithCorrectValue()
      {
        dynamic model = new ExpandoObject();
        model.a = "x";
        var sut = new Parser();

        var result = sut.Parse("{{a}}").ToList();

        Assert.AreEqual(1, result.Count);
        Assert.IsInstanceOfType(result[0], typeof (PropertyNode));
        Assert.AreEqual("x", result[0].Eval(model));
      }
    }
  }
}