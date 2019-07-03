# FullTextSearchQuery

[![NuGet version (SoftCircuits.FullTextSearchQuery)](https://img.shields.io/nuget/v/SoftCircuits.FullTextSearchQuery.svg?style=flat-square)](https://www.nuget.org/packages/SoftCircuits.FullTextSearchQuery/)

```
Install-Package SoftCircuits.FullTextSearchQuery
```

Full Text Search Query is a .NET class library to help convert a user-friendly, Google-like search term into a valid Microsoft SQL Server full-text-search query. The code attempts to gracefully handle all syntax that would cause SQL Server to generate an error.

# Details
Microsoft SQL Server provides a powerful full-text search feature. However, the syntax is rather cryptic, especially for non-programmers. Moreover, there are many conditions that can cause SQL Server to throw up an error if things aren't exactly right.

Easy Full Text Search converts a user-friendly, Google-like search term to the corresponding full-text search SQL query. Its goal is to never throw exceptions on badly formed input. It attempts to simply construct a valid query as best it can, regardless of the input.

# Input Syntax
The following list shows how various input syntaxes are interpreted.

| Term | Meaning
| ---- | ----
| abc | Find inflectional forms of abc.
| ~abc | Find thesaurus variations of abc.
| +abc | Find exact term abc.
| "abc" | Find exact term abc.
| "abc" near "def" | Find exact term abc near exact term def
| abc* | Finds words that start with abc.
| -abc | Do not include results that contain inflectional forms of abc.
| abc def | Find inflectional forms of both abc and def.
| abc or def | Find inflectional forms of either abc or def.
| &lt;abc def&gt; | Find inflectional forms of abc near def.
| abc and (def or ghi) | Find inflectional forms of both abc and either def or ghi.

# Prevent SQL Server Errors
Another goal of Easy Full Text Search is to always produce a valid SQL query. While the expression tree may be properly constructed, it may represent a query that is not supported by SQL Server. After constructing the expression tree, the code traverse the tree and takes steps to correct any conditions that would cause SQL Server to throw an error

| Term | Action Taken
| ---- | ----
| NOT term1 AND term2 | Subexpressions swapped.
| NOT term1 | Expression discarded.
| NOT term1 AND NOT term2 | Expression discarded if node is grouped (parenthesized) or is the root node; otherwise, the parent node may contain another subexpression that will make this one valid.
| term1 OR NOT term2 | Expression discarded.
| term1 NEAR NOT term2 | NEAR conjunction changed to AND.

This method converts all NEAR conjunctions to AND when either subexpression is not an InternalNode with the form TermForms.Literal.

# Usage
You can use the `FtsQuery` class to generate a search condition from the string in `text` as follows.

```c#
FtsQuery ftsQuery = new FtsQuery(true);
string SearchTerm = ftsQuery.Transform(text);
```

The resulting condition can be passed to SQL's `CONTAINS` or `CONTAINSTABLE` functions.

```sql
SELECT select_list
FROM table AS FT_TBL INNER JOIN
   CONTAINSTABLE(table, column, @SearchTerm) AS KEY_TBL
   ON FT_TBL.unique_key_column = KEY_TBL.[KEY];
```

# Stop Words (Noise Words)
One thing to be aware of is SQL Server's handling of stop words. Stop words are words such as *a*, *and*, and *the*. These words are not included in the full-text index. SQL Server does not index these words because they are very common and don't really add to the quality of the search. Since these words are not indexed, SQL Server will never find a match for these words. The result is that a search for a stop word will return no results, even though that stop word may appear in your articles.

The best way to handle this seems to be to exclude these words from the SQL query. Easy Full Text Search allows you to do this by adding stop words to the `StopWords` collection property. Stop words will not be included in the resulting query unless they are quoted, thereby preventing stop words in the query from blocking all results.

Alternatively, SQL Server provides an option for preventing the issue described above. The transform noise words option can be used to enable SQL Server to return matches even when the query contains a stop word (noise word). Set this option to 1 to enable noise word transformation.

# More Information
For more information and a discussion of the code, please see my article [Easy Full-Text Search Queries](http://www.blackbeltcoder.com/Articles/data/easy-full-text-search-queries).
