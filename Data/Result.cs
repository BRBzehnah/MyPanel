using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; } = new Error(ErrorType.None, String.Empty);

        protected Result(bool isSuccess, Error error)
        {
            if (isSuccess && error.Type != ErrorType.None)
                throw new InvalidOperationException("Successful result cannot have an error.");
            if (!isSuccess && error.Type == ErrorType.None)
                throw new InvalidOperationException("Failure result must have an error.");
            IsSuccess = isSuccess;
            Error = error;
        }
        protected Result(Error error) 
        {
            IsSuccess = false;
            Error = error;
        }

        public static Result Success() => new Result(true, new Error(ErrorType.None, String.Empty));
        public static Result Failure(Error error) => new Result(error);
    }
}
