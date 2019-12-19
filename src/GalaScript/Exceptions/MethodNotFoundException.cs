using System;

namespace GalaScript.Exceptions
{
    public class MethodNotFoundException : Exception
    {
        public string Method { get; set; }

        public override string Message => $"Method \"{Method}\" Not Found";

        public MethodNotFoundException(string method)
        {
            Method = method;
        }
    }
}
