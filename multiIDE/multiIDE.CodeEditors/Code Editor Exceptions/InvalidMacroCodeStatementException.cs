using System;

namespace multiIDE.CodeEditors
{
    [Serializable]
    public class InvalidMacroCodeStatementException : CodePreprocessingException
    {
        public InvalidMacroCodeStatementException() { }
        public InvalidMacroCodeStatementException(string message) : base(message) { }
        public InvalidMacroCodeStatementException(string message, Exception inner) : base(message, inner) { }
        protected InvalidMacroCodeStatementException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
