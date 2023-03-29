namespace Ninbus.EventBus
{
    public struct Result
    {
        public Exception? Exception { get; private set; }
        public bool IsSuccess { get; private set; }

        internal Result(bool success)
        {
            IsSuccess = success;
            Exception = null;
        }

        internal Result(Exception exception)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            IsSuccess = false;
        }

        public static Result Success()
        => new(true);

        public static Result Error(Exception exception)
            => new(exception);
    }
}
