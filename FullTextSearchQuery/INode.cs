// Copyright (c) 2020-2024 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

namespace SoftCircuits.FullTextSearchQuery
{
    /// <summary>
    /// Common interface for expression nodes.
    /// </summary>
    internal interface INode
    {
        /// <summary>
        /// Indicates this term (or both child terms) should be excluded from
        /// the results
        /// </summary>
        bool Exclude { get; set; }

        /// <summary>
        /// Indicates this term is enclosed in parentheses
        /// </summary>
        bool Grouped { get; set; }

        /// <summary>
        /// Non-nullable ToString().
        /// </summary>
        string ToString();
    }
}
