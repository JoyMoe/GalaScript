using System;

namespace GalaScript.Exceptions
{
    public class MethodNotFoundException : Exception
    {
        public string Method { get; set; }

        public MethodNotFoundException(string method)
        {
            Method = method;
        }
    }
}
