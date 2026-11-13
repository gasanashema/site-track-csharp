using System;

namespace SiteTrack.Core
{
    public class Result
    {
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; }

        protected Result(bool isSuccess, string errorMessage)
        {
            this.IsSuccess = isSuccess;
            this.ErrorMessage = errorMessage;
        }

        public static Result Success()
        {
            return new Result(true, null);
        }

        public static Result Failure(string errorMessage)
        {
            return new Result(false, errorMessage);
        }
    }

    public class Result<T> : Result
    {
        public T Value { get; private set; }

        protected Result(bool isSuccess, T value, string errorMessage)
            : base(isSuccess, errorMessage)
        {
            this.Value = value;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(true, value, null);
        }

        public new static Result<T> Failure(string errorMessage)
        {
            return new Result<T>(false, default(T), errorMessage);
        }
    }

    public class ValidationException : Exception
    {
        public string PropertyName { get; private set; }

        public ValidationException(string message)
            : base(message)
        {
            this.PropertyName = string.Empty;
        }

        public ValidationException(string propertyName, string message)
            : base(message)
        {
            this.PropertyName = propertyName;
        }
    }
}

