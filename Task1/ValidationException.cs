using System;

namespace Task1
{
    public static class ValidationException
    {
        public static Exception Create(string message)
        {
            return new Exception(message);
        }

        public static void Throw(string message)
        {
            throw Create(message);
        }

        public static void Throw(Exception exception)
        {
            throw exception;
        }

        public static void ThrowIf(bool condition, string message)
        {
            if (condition)
            {
                Throw(message);
            }
        }

        public static void ThrowIf(bool condition, Exception exception)
        {
            if (condition)
            {
                Throw(exception);
            }
        }

        public static void ThrowIfNull(object @object, string message)
        {
            ThrowIf(@object == null, message);
        }

        public static void ThrowIfNull(object @object, Exception exception)
        {
            ThrowIf(@object == null, exception);
        }
    }
}
