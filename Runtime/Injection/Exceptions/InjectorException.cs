using System;

namespace Ninito.MinJect.Injection.Exceptions
{
    public class InjectorException : Exception
    {
        public InjectorException(string message) : base(message)
        {
        }
    }
}