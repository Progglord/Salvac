using System;

namespace Salvac.Data
{
    public static class Utils
    {
        public static bool FloatingEqual(double a, double b, double epsilon)
        {
            double absA = Math.Abs(a);
            double absB = Math.Abs(b);
            double diff = Math.Abs(a - b);

            if (a == b)
                return true;
            else if (a == 0 || b == 0 || diff < double.Epsilon)
                return diff < (epsilon * double.Epsilon);
            else
                return diff / (absA + absB) < epsilon;
        }
    }
}
