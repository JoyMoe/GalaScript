using System;

namespace GalaScript.Exceptions
{
    public class MethodNotFoundException : Exception
    {
        private readonly string _method;

        public override string Message => $"Method \"{_method}\" Not Found";

        public MethodNotFoundException(string method)
        {
            _method = method;
        }
    }
}
