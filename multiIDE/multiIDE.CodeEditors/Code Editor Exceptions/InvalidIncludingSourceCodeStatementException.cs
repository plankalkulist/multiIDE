using System;

namespace multiIDE.CodeEditors
{
    [Serializable]
    public class InvalidIncludingSourceCodeStatementException : CodePreprocessingException
    {
        public InvalidIncludingSourceCodeStatementException() { }
        public InvalidIncludingSourceCodeStatementException(string message) : base(message) { }
        public InvalidIncludingSourceCodeStatementException(string message, Exception inner) : base(message, inner) { }
        protected InvalidIncludingSourceCodeStatementException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
