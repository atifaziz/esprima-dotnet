using Esprima.Ast;

namespace Esprima
{
    /// <summary>
    /// Used to save references to all function and variable declarations in a specific scope.
    /// </summary>
    public class HoistingScope
    {
        public List<FunctionDeclaration> FunctionDeclarations;
        public List<VariableDeclaration> VariableDeclarations;

        public HoistingScope(List<FunctionDeclaration> functionDeclarations,
                             List<VariableDeclaration> variableDeclarations)
        {
            FunctionDeclarations = functionDeclarations;
            VariableDeclarations = variableDeclarations;
        }
    }
}