namespace Esprima
{
    public interface IErrorHandler
    {
        bool Tolerant { get; set; }
        void Tolerate(ParserException error);
        ParserException CreateError(int index, int line, int column, string message);
        void TolerateError(int index, int line, int column, string message);
    }
}
