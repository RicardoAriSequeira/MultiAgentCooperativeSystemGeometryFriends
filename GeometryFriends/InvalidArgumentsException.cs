using System;

namespace GeometryFriends
{
    class InvalidArgumentsException : Exception
    {
        public InvalidArgumentsException(string message) : base(message) { }
    }
}
