using System.Collections.Generic;

namespace Esprima.Ast
{
    public class BreakStatement : Statement
    {
        public Identifier Label { get; }

        public BreakStatement(Identifier label) :
            base(Nodes.BreakStatement)
        {
            Label = label;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Label);
    }
}