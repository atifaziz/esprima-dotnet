using System;
using System.Collections.Generic;

namespace Esprima
{
    public class ErrorHandler : IErrorHandler
    {
        private ICollection<ParserException> _errors;

        public ICollection<ParserException> Errors =>
            _errors ?? (_errors = new List<ParserException>());

        public bool Tolerant { get; set; }

        public string Source { get; }

        public ErrorHandler() {}

        public ErrorHandler(ICollection<ParserException> errors) =>
            _errors = errors ?? throw new ArgumentNullException(nameof(errors));

        public ErrorHandler(string source) =>
            Source = source;

        public ErrorHandler(ICollection<ParserException> errors, string source)
        {
            _errors = errors ?? throw new ArgumentNullException(nameof(errors));
            Source = source;
        }

        public void Tolerate(ParserException error)
        {
            if (Tolerant)
            {
                Errors.Add(error);
            }
            else
            {
                throw error;
            }
        }

        public ParserException CreateError(int index, int line, int col, string description)
        {
            var msg = $"Line {line}': {description}";
            var error = new ParserException(msg)
            {
                Index = index,
                Column = col,
                LineNumber = line,
                Description = description,
                Source = Source
            };
            return error;
        }

        public void TolerateError(int index, int line, int col, string description)
        {
            var error = this.CreateError(index, line, col, description);
            if (Tolerant)
            {
                this.Errors.Add(error);
            }
            else
            {
                throw error;
            }
        }
    }
}
