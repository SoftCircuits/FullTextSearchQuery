# FullTextSearchQuery

[![NuGet version (SoftCircuits.FullTextSearchQuery)](https://img.shields.io/nuget/v/SoftCircuits.FullTextSearchQuery.svg?style=flat-square)](https://www.nuget.org/packages/SoftCircuits.FullTextSearchQuery/)

```
Install-Package SoftCircuits.FullTextSearchQuery
```

FullTextSearchQuery is a .NET class library that converts a user-friendly search term into a valid Microsoft SQL Server full-text-search query. The code attempts to detect and handle all cases where the query would otherwise cause SQL Server to generate an error.

# Introduction
Microsoft SQL Server provides a powerful full-text search feature. However, the syntax is rather cryptic, especially for non-programmers. Moreover, there are many conditions that can cause SQL Server to throw up an error if things aren't exactly right.

FullTextSearchQuery converts a user-friendly, Google-like search term to the corresponding full-text search SQL query condition. Its goal is to never throw exceptions on badly formed input. It simply constructs the best valid query it can from the input.

# Examples
The following list shows how various input are transformed.

| Input | Output | Description |
| ---- | ---- | ---- |
| abc | `FORMSOF(INFLECTIONAL, abc)` | Find inflectional* forms of abc.
| ~abc | `FORMSOF(THESAURUS, abc)` | Find thesaurus* variations of abc.
| "abc" | `"abc"` | Find exact term abc.
| +abc | `"abc"` | Find exact term abc.
| "abc" near "def" | `"abc" NEAR "def"` | Find exact term abc near exact term def.
| abc* | `"abc*"` | Finds words that start with abc.
| -abc def | `FORMSOF(INFLECTIONAL, def) AND NOT FORMSOF(INFLECTIONAL, abc)` | Find inflectional forms of def but not inflectional forms of abc. |
| abc def | `FORMSOF(INFLECTIONAL, abc) AND FORMSOF(INFLECTIONAL, def)` | Find inflectional forms of both abc and def.
| abc or def | `FORMSOF(INFLECTIONAL, abc) OR FORMSOF(INFLECTIONAL, def)` | Find inflectional forms of either abc or def.
| &lt;+abc +def&gt; | `"abc" NEAR "def"` | Find exact term abc near exact term def.
| abc and (def or ghi) | `FORMSOF(INFLECTIONAL, abc) AND (FORMSOF(INFLECTIONAL, def) OR FORMSOF(INFLECTIONAL, ghi))` | Find inflectional forms of both abc and either def or ghi.
* Inflectional finds all of the tenses of a word. For example, if you passed in Start, Inflectional will find Start, Started, and Starting. For nouns, Inflectional finds the single, plural, and possessive forms.
* Thesaurus finds variations of a word, Start, Begin, etc. Essentially, words that have the same meaning.

# Preventing SQL Server Errors
Even after a syntactically correct query has been generated, SQL Server can still generate an error for some queries. For example, in the table above you can see that the ouput for `-abc def` swaps the two subexpressions. This is because `NOT FORMSOF(INFLECTIONAL, abc) AND FORMSOF(INFLECTIONAL, def)` will cause an error. SQL Server does not like the `NOT` at the start. In this example, FullTextSearchQuery will swaps the two subexpressions (on either side of `AND`).

After constructing a query, FullTextSearchQuery will check for this and several other error conditions and make corrections as necessary. The following table describes these conditions.

| Term | Action Taken
| ---- | ----
| NOT term1 AND term2 | Subexpressions swapped.
| NOT term1 | Expression discarded.
| NOT term1 AND NOT term2 | Expression discarded if node is grouped (parenthesized) or is the root node; otherwise, the parent node may contain another subexpression that will make this one valid.
| term1 OR NOT term2 | Expression discarded.
| term1 NEAR NOT term2 | NEAR conjunction changed to AND.

FullTextSearchQuery converts all NEAR conjunctions to AND when either subexpression is not an InternalNode with the form TermForms.Literal.

# Usage
Use the `Transform()` method to convert a search expression to a valid SQL Server full-text search condition. This method takes a user-friendly search query and converts it to a correctly formed full-text search condition that can be passed to SQL Server's `CONTAINS` or `CONTAINSTABLE` functions. If the query contains invalid terms, the code will do what it can to return a valid search condition. If no valid terms were found, this method returns an empty string.

```c#
// Pass true to add the standard stop words
FtsQuery ftsQuery = new FtsQuery(true);
string searchTerm = ftsQuery.Transform(text);
```

```c#
// Pass extended settings to configure FtsQuery
FtsQuery ftsQuery = new FtsQuery(new FtsQuerySettings
{
	AddStandardStopWords = true,
	UseInflectionalSearch = true,
	TreatNearAsOperator = true,
	UseTrailingWildcardForAllWords = false,
	DefaultConjunction = DefaultConjunctionType.And,
	DisabledPunctuation = new[] { '-' },
	AdditionalStopWords = new[] { "car" }
});
string searchTerm = ftsQuery.Transform(text);
```

In the following SQL query example, `@SearchTerm` is a reference to the string returned from `Transform()`.

```sql
SELECT select_list
FROM table AS FT_TBL INNER JOIN
   CONTAINSTABLE(table, column, @SearchTerm) AS KEY_TBL
   ON FT_TBL.unique_key_column = KEY_TBL.[KEY];
```

# Stop Words (Noise Words)
One thing to be aware of is SQL Server's handling of stop words. Stop words are words such as *a*, *and*, and *the*. These words are not included in the full-text index. SQL Server does not index these words because they are very common and don't add to the quality of the search. Since these words are not indexed, SQL Server will never find a match for these words. The result is that a search for a stop word will return no results, even though that stop word may appear in an articles!

The best way to handle this seems to be to exclude these words from the SQL query. Easy Full Text Search allows you to do this by adding stop words to the `StopWords` collection property. Stop words will not be included in the resulting query unless they are quoted, thereby preventing stop words in the query from blocking all results.

The easiest way to add a standard list of stop words to the `StopWords` collection is to pass `true` to the `FtsQuery` constructor. (To see which words were added, you can simply inspect the `StopWords` collection.) You can modify the `StopWords` collection at any time, as needed.

Alternatively, SQL Server provides an option for preventing the issue described above. The transform noise words option can be used to enable SQL Server to return matches even when the query contains a stop word (noise word). Set this option to 1 to enable noise word transformation. See [transform noise words Server Configuration Option](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/transform-noise-words-server-configuration-option?view=sql-server-ver15) for more information.

The following query can be used to get the system stop words from a SQL Server database.

```sql
SELECT ssw.stopword, slg.name
FROM sys.fulltext_system_stopwords ssw
JOIN sys.fulltext_languages slg
ON slg.lcid = ssw.language_id
WHERE slg.lcid = 1033
```

# More Information
For more information and a discussion of the code, please see my article [Easy Full-Text Search Queries](http://www.blackbeltcoder.com/Articles/data/easy-full-text-search-queries).
