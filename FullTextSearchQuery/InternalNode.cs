// Copyright (c) 2019 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

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
        public INode LeftChild { get; set; }
        public INode RightChild { get; set; }
        public ConjunctionType Conjunction { get; set; }

        // Convert node to string
        public override string ToString()
        {
            return string.Format(Grouped ? "({0} {1} {2})" : "{0} {1} {2}",
                LeftChild.ToString(),
                Conjunction.ToString().ToUpper(),
                RightChild.ToString());
        }
    }
}
