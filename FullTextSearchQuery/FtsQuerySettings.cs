using System;

namespace SoftCircuits.FullTextSearchQuery
{
	public class FtsQuerySettings
	{
		/// <summary>
		/// If true, then standard list of FTS stopwords are added to the stopword list.
		/// </summary>
		public bool AddStandardStopWords { get; set; }

		/// <summary>
		/// Default conjunction between parsed words. Possible values: And, Or.
		/// </summary>
		public DefaultConjunctionType DefaultConjunction { get; set; } = DefaultConjunctionType.And;

		/// <summary>
		/// Use inflectional search or search by exact term by default.
		/// </summary>
		/// <remarks>
		/// Inflectional finds all of the tenses of a word.
		/// For example, if you passed in Start, Inflectional will find Start, Started, and Starting.
		/// For nouns, Inflectional finds the single, plural, and possessive forms.
		/// </remarks>
		public bool UseInflectionalSearch { get; set; } = true;

		/// <summary>
		/// Add trailing wildcard for every parsed word to search by the beginning of words.
		/// </summary>
		public bool UseTrailingWildcardForAllWords { get; set; }

		/// <summary>
		/// Treat <b>NEAR</b> term as operator.
		/// </summary>
		/// <remarks>
		/// See more about FTS NEAR operator
		/// <a href="http://docs.microsoft.com/en-us/sql/relational-databases/search/search-for-words-close-to-another-word-with-near">here.</a>
		/// </remarks>
		public bool TreatNearAsOperator { get; set; } = true;

		/// <summary>
		/// Enabled punctuation chars. If not empty then default list will be replaced with this one.
		/// </summary>
		/// <remarks>
		/// Default punctuation chars:<![CDATA[~"`!@#$%^&*()-+=[]{}\|;:,.<>?/]]>
		/// </remarks>
		public char[] EnabledPunctuation { get; set; } = Array.Empty<char>();

		/// <summary>
		/// Disable punctuation chars.
		/// </summary>
		/// <remarks>
		/// Default punctuation chars:<![CDATA[~"`!@#$%^&*()-+=[]{}\|;:,.<>?/]]>
		/// </remarks>
		public char[] DisabledPunctuation { get; set; } = Array.Empty<char>();

		/// <summary>
		/// Add additional stopwords.
		/// </summary>
		public string[] AdditionalStopWords { get; set; } = Array.Empty<string>();
	}

	/// <summary>
	/// Term conjunction types.
	/// </summary>
	public enum DefaultConjunctionType
	{
		And,
		Or
	}
}