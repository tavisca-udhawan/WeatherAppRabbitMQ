using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tavisca.Platform.Common.Tests
{
    public class CreditCardValidatorTests
    {
        [Fact]
        public void CreditCardTests_ValidNumber()
        {            
            var isValid = Validators.CreditCardValidator.Validate("4444333322221111");
            Assert.True(isValid);
        }

        [Fact]
        public void CreditCardTests_InValidNumber()
        {
            var isValid = Validators.CreditCardValidator.Validate("4444333322111");
            Assert.False(isValid);
        }

        [Fact]
        public void CreditCardTests_InValidNumberWithCharacter()
        {
            var isValid = Validators.CreditCardValidator.Validate("4444333322111A****");
            Assert.False(isValid);
        }

        [Fact]
        public void CreditCardTests_EmptyNumber()
        {
            var isValid = Validators.CreditCardValidator.Validate("");
            Assert.False(isValid);
        }

        [Fact]
        public void CreditCardTests_NullNumber()
        {
            var isValid = Validators.CreditCardValidator.Validate(null);
            Assert.False(isValid);
        }

        [Fact]
        public void CreditCardTests_InvalidNumberWithShortLength()
        {
            var isValid = Validators.CreditCardValidator.Validate("4433454");
            Assert.False(isValid);
        }

        [Fact]
        public void CreditCardTests_InvalidNumberWithMaxNumber()
        {
            var isValid = Validators.CreditCardValidator.Validate("9999999999999999999");
            Assert.False(isValid);
        }
    }
}
