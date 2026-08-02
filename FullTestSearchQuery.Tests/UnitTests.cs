using SoftCircuits.FullTextSearchQuery;

namespace FullTextSearchQuery.Tests
{
    public class UnitTests
    {
        [Fact]
        public void BasicTests()
        {
            FtsQuery query = new(true);

            // Inflectional forms
            Assert.Equal("FORMSOF(INFLECTIONAL, abc)", query.Transform("abc"));
            // Thesaurus variations
            Assert.Equal("FORMSOF(THESAURUS, abc)", query.Transform("~abc"));
            // Exact term
            Assert.Equal("\"abc\"", query.Transform("\"abc\""));
            // Exact term
            Assert.Equal("\"abc\"", query.Transform("+abc"));
            // Exact term "abc" near exact term "def"
            Assert.Equal("\"abc\" NEAR \"def\"", query.Transform("\"abc\" near \"def\""));
            // Words that start with "abc"
            Assert.Equal("\"abc*\"", query.Transform("abc*"));
            // Inflectional forms of "def" but not inflectional forms of "abc"
            Assert.Equal("FORMSOF(INFLECTIONAL, def) AND NOT FORMSOF(INFLECTIONAL, abc)", query.Transform("-abc def"));
            // Inflectional forms of both "abc" and "def"
            Assert.Equal("FORMSOF(INFLECTIONAL, abc) AND FORMSOF(INFLECTIONAL, def)", query.Transform("abc def"));
            // Exact term "abc" near exact term "def"
            Assert.Equal("\"abc\" NEAR \"def\"", query.Transform("<+abc +def>"));
            // Inflectional forms of both "abc", and either "def" or "ghi".
            Assert.Equal("FORMSOF(INFLECTIONAL, abc) AND (FORMSOF(INFLECTIONAL, def) OR FORMSOF(INFLECTIONAL, ghi))", query.Transform("abc and (def or ghi)"));
        }

        [Fact]
        public void FixupTests()
        {
            FtsQuery query = new(true);

            // Subexpressions swapped
            Assert.Equal("FORMSOF(INFLECTIONAL, term2) AND NOT FORMSOF(INFLECTIONAL, term1)", query.Transform("NOT term1 AND term2"));
            // Expression discarded
            Assert.Equal("", query.Transform("NOT term1"));
            // Expression discarded if node is grouped (parenthesized) or is the root node;
            // otherwise, the parent node may contain another subexpression that will make
            // this one valid.
            Assert.Equal("", query.Transform("NOT term1 AND NOT term2"));
            // Expression discarded
            Assert.Equal("", query.Transform("term1 OR NOT term2"));
            // NEAR conjunction changed to AND
            Assert.Equal("FORMSOF(INFLECTIONAL, term1) AND NOT FORMSOF(INFLECTIONAL, term2)", query.Transform("term1 NEAR NOT term2"));
        }

        [Fact]
        public void StopwordsTests()
        {
            FtsQuery query = new();
            Assert.Empty(query.StopWords);
            query = new(true);
            Assert.NotEmpty(query.StopWords);
        }
    }
}
