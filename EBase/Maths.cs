namespace Erde
{
    public static class Maths
    {
        public const float PI = 3.14159274F;

        public static float Min (float a_valueA, float a_valueB)
        {
            return a_valueA < a_valueB ? a_valueA : a_valueB;
        }
        public static float Max (float a_valueA, float a_valueB)
        {
            return a_valueA > a_valueB ? a_valueA : a_valueB;
        }
        public static float Clamp (float a_value, float a_min, float a_max)
        {
            return Min(a_max, Max(a_min, a_value));
        }

        public static float Lerp (float a_valueA, float a_valueB, float a_interp)
        {
            return (1 - a_interp) * a_valueA + a_interp * a_valueB;
        }
    }
}
