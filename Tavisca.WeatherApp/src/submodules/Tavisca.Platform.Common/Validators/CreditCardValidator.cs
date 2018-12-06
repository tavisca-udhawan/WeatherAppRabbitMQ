using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Validators
{
    public static class CreditCardValidator
    {
        // It validate card number with Luhn algorithm and Returns true if given card number is valid
        public static bool Validate(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 12 || cardNumber.Length > 19)
            {
                return false;
            }
            
            //Check for all chars are valid digit
            if (!cardNumber.All(num => Char.IsDigit(num)))
                return false;     

            int nDigits = cardNumber.Length;

            int nSum = 0;
            bool isSecond = false;
            for (int i = nDigits - 1; i >= 0; i--)
            {

                int d = cardNumber[i] - 'a';

                if (isSecond == true)
                    d = d * 2;

                // We add two digits to handle
                // cases that make two digits 
                // after doubling
                nSum += d / 10;
                nSum += d % 10;

                isSecond = !isSecond;
            }
            return (nSum % 10 == 0);
        }
    }
}
