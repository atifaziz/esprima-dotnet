using System.Collections.Generic;

namespace Esprima.Ast
{
    public class AssignmentPattern :
        Node,
        Expression,
        IArrayPatternElement,
        IFunctionParameter,
        PropertyValue
    {
        public INode Left  { get; }
        public INode Right { get; }

        public AssignmentPattern(INode left, INode right) :
            base(Nodes.AssignmentPattern)
        {
            Left = left;
            Right = right;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Left, Right);
    }
}
