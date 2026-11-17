namespace SportsTeamManagement.Application.Common.Models
{
    public class Result
    {
        public bool IsSuccess { get; private set; }
        public string? ErrorMessage { get; private set; }
        public List<string> Errors { get; private set; } = new();

        protected Result(bool isSuccess, string? errorMessage = null)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static Result Success() => new(true);
        public static Result Failure(string errorMessage) => new(false, errorMessage);
        public static Result Failure(List<string> errors) => new(false) { Errors = errors };

        public static Result<T> Success<T>(T value) => new(value, true);
        public static Result<T> Failure<T>(string errorMessage) => new(default, false, errorMessage);
        public static Result<T> Failure<T>(List<string> errors) => new(default, false) { Errors = errors };
    }

    public class Result<T> : Result
    {
        public T? Value { get; private set; }

        internal Result(T? value, bool isSuccess, string? errorMessage = null) : base(isSuccess, errorMessage)
        {
            Value = value;
        }
    }
}
