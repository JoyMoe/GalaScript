namespace GalaScript.Internal
{
    internal static partial class InternalOperations
    {
        public static class ArithmeticOperations
        {
            public static decimal Add(decimal x, decimal y) => x + y;

            public static decimal Mul(decimal x, decimal y) => x * y;

            public static decimal Div(decimal x, decimal y) => x / y;

            public static bool Gt(decimal x, decimal y) => x > y;

            public static bool Ge(decimal x, decimal y) => x >= y;

            public static bool Lt(decimal x, decimal y) => x < y;

            public static bool Le(decimal x, decimal y) => x <= y;

            public static int Cmp(decimal x, decimal y) => x switch
            {
                _ when x > y => 1,
                _ when x < y => -1,
                _ => 0
            };

            public static bool Eq(object x, object y) => x.Equals(y);

            public static bool Ne(object x, object y) => !Eq(x, y);
        }
    }
}
