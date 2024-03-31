// Copyright (c) 2020-2024 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//
using SoftCircuits.FullTextSearchQuery;

namespace FullTextSearchQueryTests
{
    public class FtsTests
    {
        [Test]
        public void BasicTests()
        {
            FtsQuery query = new(true);

            Assert.Multiple(() =>
            {
                // Inflectional forms
                Assert.That(query.Transform("abc"), Is.EqualTo("FORMSOF(INFLECTIONAL, abc)"));
                // Thesaurus variations
                Assert.That(query.Transform("~abc"), Is.EqualTo("FORMSOF(THESAURUS, abc)"));
                // Exact term
                Assert.That(query.Transform("\"abc\""), Is.EqualTo("\"abc\""));
                // Exact term
                Assert.That(query.Transform("+abc"), Is.EqualTo("\"abc\""));
                // Exact term "abc" near exact term "def"
                Assert.That(query.Transform("\"abc\" near \"def\""), Is.EqualTo("\"abc\" NEAR \"def\""));
                // Words that start with "abc"
                Assert.That(query.Transform("abc*"), Is.EqualTo("\"abc*\""));
                // Inflectional forms of "def" but not inflectional forms of "abc"
                Assert.That(query.Transform("-abc def"), Is.EqualTo("FORMSOF(INFLECTIONAL, def) AND NOT FORMSOF(INFLECTIONAL, abc)"));
                // Inflectional forms of both "abc" and "def"
                Assert.That(query.Transform("abc def"), Is.EqualTo("FORMSOF(INFLECTIONAL, abc) AND FORMSOF(INFLECTIONAL, def)"));
                // Exact term "abc" near exact term "def"
                Assert.That(query.Transform("<+abc +def>"), Is.EqualTo("\"abc\" NEAR \"def\""));
                // Inflectional forms of both "abc", and either "def" or "ghi".
                Assert.That(query.Transform("abc and (def or ghi)"), Is.EqualTo("FORMSOF(INFLECTIONAL, abc) AND (FORMSOF(INFLECTIONAL, def) OR FORMSOF(INFLECTIONAL, ghi))"));
            });
        }

        [Test]
        public void FixupTests()
        {
            FtsQuery query = new(true);

            Assert.Multiple(() =>
            {
                // Subexpressions swapped
                Assert.That(query.Transform("NOT term1 AND term2"), Is.EqualTo("FORMSOF(INFLECTIONAL, term2) AND NOT FORMSOF(INFLECTIONAL, term1)"));
                // Expression discarded
                Assert.That(query.Transform("NOT term1"), Is.EqualTo(""));
                // Expression discarded if node is grouped (parenthesized) or is the root node;
                // otherwise, the parent node may contain another subexpression that will make
                // this one valid.
                Assert.That(query.Transform("NOT term1 AND NOT term2"), Is.EqualTo(""));
                // Expression discarded
                Assert.That(query.Transform("term1 OR NOT term2"), Is.EqualTo(""));
                // NEAR conjunction changed to AND
                Assert.That(query.Transform("term1 NEAR NOT term2"), Is.EqualTo("FORMSOF(INFLECTIONAL, term1) AND NOT FORMSOF(INFLECTIONAL, term2)"));
            });
        }

        [Test]
        public void StopwordsTests()
        {
            FtsQuery query = new();
            Assert.That(query.StopWords, Has.Count.EqualTo(0));
            query = new(true);
            Assert.That(query.StopWords, Is.Not.Count.EqualTo(0));
        }
    }
}
