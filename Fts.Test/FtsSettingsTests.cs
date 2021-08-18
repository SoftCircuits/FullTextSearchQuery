using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftCircuits.FullTextSearchQuery;

namespace Fts.Test
{
	[TestClass]
	public class FtsSettingsTests
	{
		[TestMethod]
		public void WordsJoinedWithOr_WhenSettingTurnedOn()
		{
			var query = new FtsQuery(new FtsQuerySettings { DefaultConjunction = DefaultConjunctionType.Or });

			var actual = query.Transform("\"dk product\" dkp dkp123");

			const string expected = "\"dk product\" OR FORMSOF(INFLECTIONAL, dkp) OR FORMSOF(INFLECTIONAL, dkp123)";
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void TrailingWildcardAddedToWords_WhenSettingTurnedOn()
		{
			var query = new FtsQuery(new FtsQuerySettings { UseTrailingWildcardForAllWords = true });

			var actual = query.Transform("\"dk product\" dkp dkp123");

			const string expected = "\"dk product\" AND \"dkp*\" AND \"dkp123*\"";
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void InflectionalSearchIsNotUsed_WhenDefaultTermFormIsLiteral()
		{
			var query = new FtsQuery(new FtsQuerySettings { UseInflectionalSearch = false });

			Assert.AreEqual("\"abc\"", query.Transform("abc"));

			Assert.AreEqual("\"abc\" AND \"def\"", query.Transform("abc def"));

			Assert.AreEqual("\"def\" AND NOT \"abc\"", query.Transform("-abc def"));

			Assert.AreEqual("\"abc\" AND (\"def\" OR \"ghi\")", query.Transform("abc and (def or ghi)"));
		}

		[TestMethod]
		public void HyphenIsNotParsedAsPunctuation_WhenHyphenIsDisabled()
		{
			var query = new FtsQuery(new FtsQuerySettings { DisabledPunctuation = new[] { '-' } });

			var actual = query.Transform("t-shirt");

			const string expected = "FORMSOF(INFLECTIONAL, t-shirt)";
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void TildeIsNotParsedAsPunctuation_WhenTildeIsDisabled()
		{
			var query = new FtsQuery(new FtsQuerySettings { DisabledPunctuation = new[] { '~' } });

			var actual = query.Transform("~abc");

			const string expected = "FORMSOF(INFLECTIONAL, ~abc)";
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void NearIsNotParsedAsPunctuation_WhenSingleAngleQuotationMarks_andPlusAreDisabled()
		{
			var query = new FtsQuery(new FtsQuerySettings { DisabledPunctuation = new[] { '<', '>', '+' } });

			var actual = query.Transform("<+abc +def>");

			const string expected = "FORMSOF(INFLECTIONAL, <+abc) AND FORMSOF(INFLECTIONAL, +def>)";
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void NearIsNotParsedAsOperator_WhenItsDisabledInSettings()
		{
			var query = new FtsQuery(new FtsQuerySettings { TreatNearAsOperator = false });

			var actual = query.Transform("\"abc\" NEAR \"def\"");

			const string expected = "\"abc\" AND FORMSOF(INFLECTIONAL, NEAR) AND \"def\"";
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void TrailingWildcardAddedToWords_WhenDefaultTermFormIsLiteral_andTrailingWildcardEnabled()
		{
			var query = new FtsQuery(new FtsQuerySettings
			{
				UseInflectionalSearch = false,
				UseTrailingWildcardForAllWords = true
			});

			var actual = query.Transform("abc def ghi*");

			const string expected = "\"abc*\" AND \"def*\" AND \"ghi*\"";
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void TrailingWildcardIsNotAddedToAnd_WhenDefaultTermFormIsLiteral_andTrailingWildcardEnabled()
		{
			var query = new FtsQuery(new FtsQuerySettings
			{
				UseInflectionalSearch = false,
				UseTrailingWildcardForAllWords = true
			});

			var actual = query.Transform("abc AND def");

			const string expected = "\"abc*\" AND \"def*\"";
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void TrailingWildcardIsNotAddedToOr_WhenDefaultTermFormIsLiteral_andTrailingWildcardEnabled()
		{
			var query = new FtsQuery(new FtsQuerySettings
			{
				UseInflectionalSearch = false,
				UseTrailingWildcardForAllWords = true
			});

			var actual = query.Transform("abc OR def");

			const string expected = "\"abc*\" OR \"def*\"";
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void TrailingWildcardIsNotAddedToAndOr_WhenDefaultTermFormIsLiteral_andTrailingWildcardEnabled()
		{
			var query = new FtsQuery(new FtsQuerySettings
			{
				UseInflectionalSearch = false,
				UseTrailingWildcardForAllWords = true
			});

			var actual = query.Transform("abc and (def or ghi)");

			const string expected = "\"abc*\" AND (\"def*\" OR \"ghi*\")";
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void WordsJoinedWithOr_andUsedTrailingWildcard_WhenBothSettingsTurnedOn()
		{
			var query = new FtsQuery(new FtsQuerySettings
			{
				DefaultConjunction = DefaultConjunctionType.Or,
				UseTrailingWildcardForAllWords = true
			});

			var actual = query.Transform("\"dk product\" dkp dkp123");

			const string expected = "\"dk product\" OR \"dkp*\" OR \"dkp123*\"";
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void TrailingWildcardAddedToAllWords_andPunctuationSkipped()
		{
			var query = new FtsQuery(new FtsQuerySettings
			{
				DefaultConjunction = DefaultConjunctionType.And,
				UseInflectionalSearch = false,
				UseTrailingWildcardForAllWords = true,
				DisabledPunctuation = new[] { '~', '-', '+', '<', '>' }
			});

			var actual = query.Transform("\"dk product\" dkp OR <+dkp+123+> -ab-c AND ~def*");

			const string expected = "\"dk product\" AND \"dkp*\" OR \"<+dkp+123+>*\" AND \"-ab-c*\" AND \"~def*\"";
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void QuotesAddedToWord_WhenQuotesEnabled()
		{
			var query = new FtsQuery(new FtsQuerySettings { EnabledPunctuation = new[] { '"' } });

			var actual = query.Transform("\"dk product\" dkp123");

			const string expected = "\"dk product\" AND FORMSOF(INFLECTIONAL, dkp123)";
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void ParenthesesParsedAsPunctuation_WhenParenthesesEnabledInSettings()
		{
			var query = new FtsQuery(new FtsQuerySettings
			{
				UseInflectionalSearch = false,
				EnabledPunctuation = new[] { '(' }
			});

			var actual = query.Transform("abc and (def or ghi)");

			const string expected = "\"abc\" AND (\"def\" OR \"ghi\")";
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void AdditionalStopWordsUsed_WhenStandardStopWordsDisabled()
		{
			var additionalStopWords = new[] { "aa", "bb", "cc" };
			var query = new FtsQuery(new FtsQuerySettings { AdditionalStopWords = additionalStopWords });

			var actual = query.Transform("aa AND cc");

			const string expected = "";
			Assert.AreEqual(expected, actual);
			Assert.AreEqual(3, query.StopWords.Count);
		}

		[TestMethod]
		public void AdditionalStopWordsUsed_WhenStandardStopWordsAdded()
		{
			var additionalStopWords = new[] { "aa", "bb", "cc" };
			var query = new FtsQuery(new FtsQuerySettings
				{ AddStandardStopWords = true, AdditionalStopWords = additionalStopWords });

			var actual = query.Transform("aa AND cc");

			const string expected = "";
			Assert.AreEqual(expected, actual);
			Assert.IsTrue(query.StopWords.Count > 3, "Expected actualCount to be greater than 3.");
		}
	}
}