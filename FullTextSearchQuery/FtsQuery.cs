// Copyright (c) 2019-2021 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftCircuits.FullTextSearchQuery
{
    /// <summary>
    /// Query term forms.
    /// </summary>
    internal enum TermForm
    {
        Inflectional,
        Thesaurus,
        Literal,
    }

    /// <summary>
    /// Term conjunction types.
    /// </summary>
    internal enum ConjunctionType
    {
        And,
        Or,
        Near,
    }

    /// <summary>
    /// Class to convert user-friendly search term to SQL Server full-text search syntax.
    /// Supports a Google-like syntax as described in the remarks. No exceptions are thrown
    /// for badly formed input. The code simply constructs the best query it can.
    /// </summary>
    /// <remarks>
    /// The following list shows how various syntaxes are interpreted.
    /// 
    /// abc                     Find inflectional forms of abc
    /// ~abc                    Find thesaurus variations of abc
    /// "abc"                   Find exact term abc
    /// +abc                    Find exact term abc
    /// "abc" near "def"        Find exact term abc near exact term def
    /// abc*                    Finds words that start with abc
    /// -abc def                Find inflectional forms of def but not inflectional forms of abc
    /// abc def                 Find inflectional forms of both abc and def
    /// abc or def              Find inflectional forms of either abc or def
    /// <+abc +def>             Find exact term abc near exact term def
    /// abc and (def or ghi)    Find inflectional forms of both abc and either def or ghi
    /// </remarks>
    public class FtsQuery
    {
        // Characters not allowed in unquoted search terms
        protected readonly string Punctuation = "~\"`!@#$%^&*()-+=[]{}\\|;:,.<>?/";

        /// <summary>
        /// Collection of stop words. These words will not
        /// be included in the resulting query unless quoted.
        /// </summary>
        public HashSet<string> StopWords { get; private set; }

        /// <summary>
        /// Fts settings to change default behavior.
        /// </summary>
        private FtsQuerySettings _settings;

        /// <summary>
        /// Constructs an <see cref="FtsQuery"></see> instance.
        /// </summary>
        /// <param name="addStandardStopWords">If true, the standard list of stopwords
        /// are added to the stopword list.</param>
        public FtsQuery(bool addStandardStopWords = false)
            : this(new FtsQuerySettings{AddStandardStopWords = addStandardStopWords})
        {
        }

        /// <summary>
        /// Constructs an <see cref="FtsQuery"></see> instance.
        /// </summary>
        /// <param name="settings">Settings used to change default behavior.</param>
        public FtsQuery(FtsQuerySettings settings)
        {
            _settings = settings;

            StopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (settings.AddStandardStopWords)
            {
                foreach (string stopWord in StandardStopWords.StopWords)
                    StopWords.Add(stopWord);
            }

            foreach (string stopWord in settings.AdditionalStopWords)
                StopWords.Add(stopWord);

            if (_settings.EnabledPunctuation.Any())
            {
                Punctuation = string.Empty;
                foreach (var enabledChar in _settings.EnabledPunctuation)
                {
                    Punctuation += enabledChar.ToString();
                }
            }

            foreach (var disabledChar in _settings.DisabledPunctuation)
            {
                Punctuation = Punctuation.Replace(disabledChar.ToString(), string.Empty);
            }
        }

        /// <summary>
        /// Determines if the given word has been identified as a stop word.
        /// </summary>
        /// <param name="word">Word to test.</param>
        protected bool IsStopWord(string word) => StopWords.Contains(word);

        /// <summary>
        /// Converts a search expression to a valid SQL Server full-text search
        /// condition.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method takes a search query and converts it to a correctly formed
        /// full-text-search condition that can be passed to SQL Server constructs
        /// like <c>CONTAINSTABLE</c>.
        /// </para>
        /// <para>
        /// If the query contains invalid terms, the code will do what it can to
        /// return a valid search condition. If no valid terms were found, this
        /// method returns an empty string.
        /// </para>
        /// </remarks>
        /// <param name="query">Search term to be converted.</param>
        /// <returns>A valid full-text search query condition or an empty string
        /// if a valid condition was not possible.</returns>
        public string Transform(string query)
        {
            INode? node = ParseNode(query, (ConjunctionType)_settings.DefaultConjunction);
            node = FixUpExpressionTree(node, true);
            return node?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Parses a query segment and converts it to an expression
        /// tree.
        /// </summary>
        /// <param name="query">Query segment to be converted.</param>
        /// <param name="defaultConjunction">Implicit conjunction type.</param>
        /// <returns>Root node of expression tree.</returns>
        internal INode? ParseNode(string? query, ConjunctionType defaultConjunction)
        {
            ConjunctionType conjunction = defaultConjunction;
            TermForm termForm = _settings.UseInflectionalSearch ? TermForm.Inflectional : TermForm.Literal;
            bool termExclude = false;
            bool resetState = true;
            INode? root = null;
            INode? node;
            string term;

            ParsingHelper parser = new ParsingHelper(query);
            while (!parser.EndOfText)
            {
                if (resetState)
                {
                    // Reset modifiers
                    conjunction = defaultConjunction;
                    termForm = _settings.UseInflectionalSearch ? TermForm.Inflectional : TermForm.Literal;
                    termExclude = false;
                    resetState = false;
                }

                parser.SkipWhitespace();
                if (parser.EndOfText)
                    break;

                char ch = parser.Peek();
                if (Punctuation.Contains(ch))
                {
                    switch (ch)
                    {
                        case '"':
                        case '\'':
                            termForm = TermForm.Literal;
                            parser.MoveAhead();
                            term = parser.ParseWhile(c => c != ch);
                            root = AddNode(root, term.Trim(), termForm, termExclude, conjunction);
                            resetState = true;
                            break;
                        case '(':
                            // Parse parentheses block
                            term = ExtractBlock(parser, '(', ')');
                            node = ParseNode(term, defaultConjunction);
                            root = AddNode(root, node, conjunction, true);
                            resetState = true;
                            break;
                        case '<':
                            // Parse angle brackets block
                            term = ExtractBlock(parser, '<', '>');
                            node = ParseNode(term, ConjunctionType.Near);
                            root = AddNode(root, node, conjunction);
                            resetState = true;
                            break;
                        case '-':
                            // Match when next term is not present
                            termExclude = true;
                            break;
                        case '+':
                            // Match next term exactly
                            termForm = TermForm.Literal;
                            break;
                        case '~':
                            // Match synonyms of next term
                            termForm = TermForm.Thesaurus;
                            break;
                        default:
                            break;
                    }
                    // Advance to next character
                    parser.MoveAhead();
                }
                else
                {
                    // Parse this query term
                    term = parser.ParseWhile(c => !Punctuation.Contains(c) && !char.IsWhiteSpace(c));

                    // Allow trailing wildcard
                    if (parser.Peek() == '*')
                    {
                        term += parser.Peek();
                        parser.MoveAhead();
                        termForm = TermForm.Literal;
                        root = AddNode(root, term, termForm, termExclude, conjunction);
                        resetState = true;
                        continue;
                    }

                    // Interpret term
                    StringComparer comparer = StringComparer.OrdinalIgnoreCase;
                    if (comparer.Compare(term, "AND") == 0)
                        conjunction = ConjunctionType.And;
                    else if (comparer.Compare(term, "OR") == 0)
                        conjunction = ConjunctionType.Or;
                    else if (_settings.TreatNearAsOperator && comparer.Compare(term, "NEAR") == 0)
                        conjunction = ConjunctionType.Near;
                    else if (comparer.Compare(term, "NOT") == 0)
                        termExclude = true;
                    else if (_settings.UseTrailingWildcardForAllWords)
                    {
                        term += '*';
                        termForm = TermForm.Literal;
                        root = AddNode(root, term, termForm, termExclude, conjunction);
                        resetState = true;
                    }
                    else
                    {
                        root = AddNode(root, term, termForm, termExclude, conjunction);
                        resetState = true;
                    }
                }
            }
            return root;
        }

        /// <summary>
        /// Fixes any portions of the expression tree that would produce an invalid SQL Server full-text
        /// query.
        /// </summary>
        /// <param name="node">Node to fix up</param>
        /// <param name="isRoot">True if node is the tree's root node</param>
        /// <remarks>
        /// While our expression tree may be properly constructed, it may represent a query that
        /// is not supported by SQL Server. This method traverses the expression tree and corrects
        /// problem expressions as described below.
        /// 
        ///     NOT term1 AND term2         Subexpressions swapped.
        ///     NOT term1                   Expression discarded.
        ///     NOT term1 AND NOT term2     Expression discarded if node is grouped (parenthesized)
        ///                                 or is the root node; otherwise, the parent node may
        ///                                 contain another subexpression that will make this one
        ///                                 valid.
        ///     term1 OR NOT term2          Expression discarded.
        ///     term1 NEAR NOT term2        NEAR conjunction changed to AND. *
        ///
        /// * This method converts all NEAR conjunctions to AND when either subexpression is not
        /// an InternalNode with the form TermForms.Literal.
        /// </remarks>
        internal INode? FixUpExpressionTree(INode? node, bool isRoot = false)
        {
            // Test for empty expression tree
            if (node == null) return null;

            // Special handling for internal nodes
            if (node is InternalNode internalNode)
            {
                // Fix up child nodes
                internalNode.LeftChild = FixUpExpressionTree(internalNode.LeftChild);
                internalNode.RightChild = FixUpExpressionTree(internalNode.RightChild);

                // Correct subexpressions incompatible with conjunction type
                if (internalNode.Conjunction == ConjunctionType.Near)
                {
                    // If either subexpression is incompatible with NEAR conjunction then change to AND
                    if (IsInvalidWithNear(internalNode.LeftChild) || IsInvalidWithNear(internalNode.RightChild))
                        internalNode.Conjunction = ConjunctionType.And;
                }
                else if (internalNode.Conjunction == ConjunctionType.Or)
                {
                    // Eliminate subexpressions not valid with OR conjunction
                    if (IsInvalidWithOr(internalNode.LeftChild))
                        internalNode.LeftChild = null;
                    if (IsInvalidWithOr(internalNode.RightChild))
                        internalNode.LeftChild = null;
                }

                // Handle eliminated child expressions
                if (internalNode.LeftChild == null && internalNode.RightChild == null)
                {
                    // Eliminate parent node if both child nodes were eliminated
                    return null;
                }
                else if (internalNode.LeftChild == null)
                {
                    // Child1 eliminated so return only Child2
                    node = internalNode.RightChild;
                }
                else if (internalNode.RightChild == null)
                {
                    // Child2 eliminated so return only Child1
                    node = internalNode.LeftChild;
                }
                else
                {
                    // Determine if entire expression is an exclude expression
                    internalNode.Exclude = (internalNode.LeftChild.Exclude && internalNode.RightChild.Exclude);
                    // If only first child expression is an exclude expression,
                    // then simply swap child expressions
                    if (!internalNode.Exclude && internalNode.LeftChild.Exclude)
                    {
                        var temp = internalNode.LeftChild;
                        internalNode.LeftChild = internalNode.RightChild;
                        internalNode.RightChild = temp;
                    }
                }
            }
            // Eliminate expression group if it contains only exclude expressions
            if (node == null || ((node.Grouped || isRoot) && node.Exclude))
                return null;
            return node;
        }

        /// <summary>
        /// Determines if the specified node is invalid on either side of a NEAR conjuction.
        /// </summary>
        /// <param name="node">Node to test</param>
        internal static bool IsInvalidWithNear(INode? node)
        {
            // NEAR is only valid with TerminalNodes with form TermForms.Literal
            return node is not TerminalNode terminalNode || terminalNode.TermForm != TermForm.Literal;
        }

        /// <summary>
        /// Determines if the specified node is invalid on either side of an OR conjunction.
        /// </summary>
        /// <param name="node">Node to test</param>
        internal static bool IsInvalidWithOr(INode? node)
        {
            // OR is only valid with non-null, non-excluded (NOT) subexpressions
            return node == null || node.Exclude == true;
        }

        /// <summary>
        /// Creates an expression node and adds it to the give tree.
        /// </summary>
        /// <param name="root">Root node of expression tree.</param>
        /// <param name="term">Term for this node.</param>
        /// <param name="termForm">Indicates form of this term.</param>
        /// <param name="termExclude">Indicates if this is an excluded term.</param>
        /// <param name="conjunction">Conjunction used to join with other nodes.</param>
        /// <returns>The new root node.</returns>
        internal INode? AddNode(INode? root, string term, TermForm termForm, bool termExclude, ConjunctionType conjunction)
        {
            if (term.Length > 0 && !IsStopWord(term))
            {
                INode node = new TerminalNode
                {
                    Term = term,
                    TermForm = termForm,
                    Exclude = termExclude
                };
                root = AddNode(root, node, conjunction);
            }
            return root;
        }

        /// <summary>
        /// Adds an expression node to the given tree.
        /// </summary>
        /// <param name="root">Root node of expression tree</param>
        /// <param name="node">Node to add</param>
        /// <param name="conjunction">Conjunction used to join with other nodes</param>
        /// <returns>The new root node</returns>
        internal static INode? AddNode(INode? root, INode? node, ConjunctionType conjunction, bool group = false)
        {
            if (node != null)
            {
                node.Grouped = group;
                if (root != null)
                    root = new InternalNode
                    {
                        LeftChild = root,
                        RightChild = node,
                        Conjunction = conjunction
                    };
                else
                    root = node;
            }
            return root;
        }

        /// <summary>
        /// Extracts a block of text delimited by the specified open and close
        /// characters. It is assumed the parser is positioned at an
        /// occurrence of the open character. The open and closing characters
        /// are not included in the returned string. On return, the parser is
        /// positioned at the closing character or at the end of the text if
        /// the closing character was not found.
        /// </summary>
        /// <param name="parser">ParsingHelper object</param>
        /// <param name="openChar">Start-of-block delimiter</param>
        /// <param name="closeChar">End-of-block delimiter</param>
        /// <returns>The extracted text</returns>
        internal static string ExtractBlock(ParsingHelper parser, char openChar, char closeChar)
        {
            // Track delimiter depth
            int depth = 1;

            // Extract characters between delimiters
            parser.MoveAhead();
            int start = parser.Index;
            while (!parser.EndOfText)
            {
                char ch = parser.Peek();
                if (ch == openChar)
                {
                    // Increase block depth
                    depth++;
                }
                else if (ch == closeChar)
                {
                    // Decrease block depth
                    depth--;
                    // Test for end of block
                    if (depth == 0)
                        break;
                }
                else if (ch == '"' || ch == '\'')
                {
                    // Don't count delimiters within quoted text
                    parser.MoveAhead();
                    parser.SkipWhile(c => c != ch);
                }
                // Move to next character
                parser.MoveAhead();
            }
            return parser.Extract(start, parser.Index);
        }
    }
}
