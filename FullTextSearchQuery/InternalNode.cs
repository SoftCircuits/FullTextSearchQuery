// Copyright (c) 2019-2021 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using System.Diagnostics;

namespace SoftCircuits.FullTextSearchQuery
{
    /// <summary>
    /// Internal (non-leaf) expression node class.
    /// </summary>
    internal class InternalNode : INode
    {
        // Interface members
        public bool Exclude { get; set; }
        public bool Grouped { get; set; }

        // Class members
        public INode? LeftChild { get; set; }
        public INode? RightChild { get; set; }
        public ConjunctionType Conjunction { get; set; }

        // Convert node to string
        public override string ToString()
        {
            Debug.Assert(LeftChild != null && RightChild != null);
            if (LeftChild != null && RightChild != null)
            {
                return string.Format(Grouped ? "({0} {1} {2})" : "{0} {1} {2}",
                    LeftChild.ToString(),
                    Conjunction.ToString().ToUpper(),
                    RightChild.ToString());
            }

            if (LeftChild != null)
                return LeftChild.ToString();
            if (RightChild != null)
                return RightChild.ToString();
            return string.Empty;
        }
    }
}
