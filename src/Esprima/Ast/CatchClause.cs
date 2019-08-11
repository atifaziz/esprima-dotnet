using System.Collections.Generic;

namespace Esprima.Ast
{
    public class CatchClause : Statement
    {
        public IArrayPatternElement Param { get; } // BindingIdentifier | BindingPattern;
        public  BlockStatement Body { get; }

        public CatchClause(IArrayPatternElement param, BlockStatement body) :
            base(Nodes.CatchClause)
        {
            Param = param;
            Body = body;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Param, Body);
    }
}