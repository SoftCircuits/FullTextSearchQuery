// Copyright (c) 2019-2021 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftCircuits.FullTextSearchQuery;

namespace Fts.Test
{
    [TestClass]
    public class FtsTests
    {
        [TestMethod]
        public void BasicTests()
        {
            FtsQuery query = new FtsQuery(true);

            // Inflectional forms
            Assert.AreEqual("FORMSOF(INFLECTIONAL, abc)", query.Transform("abc"));
            // Thesaurus variations
            Assert.AreEqual("FORMSOF(THESAURUS, abc)", query.Transform("~abc"));
            // Exact term
            Assert.AreEqual("\"abc\"", query.Transform("\"abc\""));
            // Exact term
            Assert.AreEqual("\"abc\"", query.Transform("+abc"));
            // Exact term "abc" near exact term "def"
            Assert.AreEqual("\"abc\" NEAR \"def\"", query.Transform("\"abc\" near \"def\""));
            // Words that start with "abc"
            Assert.AreEqual("\"abc*\"", query.Transform("abc*"));
            // Inflectional forms of "def" but not inflectional forms of "abc"
            Assert.AreEqual("FORMSOF(INFLECTIONAL, def) AND NOT FORMSOF(INFLECTIONAL, abc)", query.Transform("-abc def"));
            // Inflectional forms of both "abc" and "def"
            Assert.AreEqual("FORMSOF(INFLECTIONAL, abc) AND FORMSOF(INFLECTIONAL, def)", query.Transform("abc def"));
            // Exact term "abc" near exact term "def"
            Assert.AreEqual("\"abc\" NEAR \"def\"", query.Transform("<+abc +def>"));
            // Inflectional forms of both "abc", and either "def" or "ghi".
            Assert.AreEqual("FORMSOF(INFLECTIONAL, abc) AND (FORMSOF(INFLECTIONAL, def) OR FORMSOF(INFLECTIONAL, ghi))", query.Transform("abc and (def or ghi)"));
        }

        [TestMethod]
        public void FixupTests()
        {
            FtsQuery query = new FtsQuery(true);

            // Subexpressions swapped
            Assert.AreEqual("FORMSOF(INFLECTIONAL, term2) AND NOT FORMSOF(INFLECTIONAL, term1)", query.Transform("NOT term1 AND term2"));
            // Expression discarded
            Assert.AreEqual("", query.Transform("NOT term1"));
            // Expression discarded if node is grouped (parenthesized) or is the root node;
            // otherwise, the parent node may contain another subexpression that will make
            // this one valid.
            Assert.AreEqual("", query.Transform("NOT term1 AND NOT term2"));
            // Expression discarded
            Assert.AreEqual("", query.Transform("term1 OR NOT term2"));
            // NEAR conjunction changed to AND
            Assert.AreEqual("FORMSOF(INFLECTIONAL, term1) AND NOT FORMSOF(INFLECTIONAL, term2)", query.Transform("term1 NEAR NOT term2"));
        }

        [TestMethod]
        public void StopwordsTests()
        {
            FtsQuery query = new FtsQuery();
            Assert.AreEqual(0, query.StopWords.Count);
            query = new FtsQuery(true);
            Assert.AreNotEqual(0, query.StopWords.Count);
        }
    }
}
