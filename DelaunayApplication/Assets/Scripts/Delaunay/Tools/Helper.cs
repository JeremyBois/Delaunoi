
namespace Delaunay.Tools
{
    public static class Helper
    {
        private const string _basesList = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// Convert a decimal number (base 10) to any other base up to 36.
        /// </summary>
        public static string ChangeBase(int number, int b)
        {
            if (number < b)
                return _basesList[number].ToString();
            else
            {
                return ChangeBase(number / b, b) + _basesList[number % b];
            }
        }

        /// <summary>
        /// Swap to variables without having to explicitly declare a temporary one.
        /// </summary>
        public static void Swap<T> (ref T left, ref T right)
        {
            T temp = left;
            left = right;
            right = temp;
        }
    }
}

