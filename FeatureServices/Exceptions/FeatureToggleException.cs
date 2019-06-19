using System;
using System.Runtime.Serialization;

namespace FeatureServices.Exceptions
{
    [Serializable]
    public class FeatureToggleException : Exception
    {
        public FeatureToggleException()
        {

        }
        public FeatureToggleException(string message) : base(message)
        {
        }

        public FeatureToggleException(string message, Exception exception) : base(message, exception)
        {
        }

        protected FeatureToggleException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}