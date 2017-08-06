using System;

namespace multiIDE
{
    [Serializable]
    public class CodePreprocessingException : Exception
    {
        public CodePreprocessingException() { }
        public CodePreprocessingException(string message) : base(message) { }
        public CodePreprocessingException(string message, Exception inner) : base(message, inner){ }
        protected CodePreprocessingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
