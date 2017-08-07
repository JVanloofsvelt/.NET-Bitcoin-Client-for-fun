using System;

namespace BTCClient
{
    public class InvalidHashSizeException : ArgumentException
    {
        public InvalidHashSizeException(string paramName) : 
            base($"Value should contain {DoubleSHA256.HashSize} bytes", paramName)
        {
        }
    }

    public class ArgumentTooLongException : ArgumentException
    {
        public ArgumentTooLongException(string paramName, int maxLength) :
            base($"Value should have a maximum length of {maxLength}", paramName)
        {

        }
    }
}
