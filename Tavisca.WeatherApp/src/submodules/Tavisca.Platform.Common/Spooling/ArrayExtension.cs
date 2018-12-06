using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common
{
    public delegate TOutput Converter<in TInput, out TOutput>(TInput input);

    public static class ArrayExtension
    {
        public static TOutput[] ConvertAll<TInput, TOutput>(TInput[] array, Converter<TInput, TOutput> converter)
        {
            var outputArray = array.Select(inputItem => converter(inputItem));
            return outputArray.ToArray();
        }

        public static T[] FindAll<T>(T[] array, Predicate<T> match)
        {
            var outputArray = array.Where(x => match(x));
            return outputArray.ToArray();
        }
    }
}
