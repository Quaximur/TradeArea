namespace TradeArea.Shared
{
    /// <summary>
    /// Структура-обёртка над результатом запроса (без возвращаемого значения). 
    /// Содержит информацию об успешности запроса.
    /// В случае ошибки может содержать сообщение об ошибке.
    /// </summary>
    public struct LoadingState
    {
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; }
        public LoadingFlag LoadingFlag { get; private set; }

        public static LoadingState Loading()
        {
            return new LoadingState
            {
                LoadingFlag = LoadingFlag.Loading
            };
        }

        public static LoadingState Success(LoadingFlag code = LoadingFlag.Success)
        {
            return new LoadingState
            {
                IsSuccess = true,
                LoadingFlag = code
            };
        }

        public static LoadingState Fail(LoadingFlag code = LoadingFlag.Fail)
        {
            return new LoadingState
            {
                IsSuccess = false,
                LoadingFlag = code
            };
        }

        public static LoadingState Fail(string message, LoadingFlag code = LoadingFlag.Fail)
        {
            return new LoadingState
            {
                IsSuccess = false,
                ErrorMessage = message,
                LoadingFlag = code
            };
        }
    }

    /// <summary>
    /// Структура-обёртка над результатом запроса (с возвращаемым значением). 
    /// Содержит информацию об успешности запроса.
    /// В случае успеха содержит значение.
    /// В случае ошибки может содержать сообщение об ошибке.
    /// </summary>
    public struct LoadingState<T>
    {
        public T Value { get; private set; }
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; }
        public LoadingFlag LoadingFlag { get; private set; }

        public static LoadingState<T> Loading()
        {
            return new LoadingState<T>
            {
                LoadingFlag = LoadingFlag.Loading
            };
        }

        public static LoadingState<T> Success(T value, LoadingFlag code = LoadingFlag.Success)
        {
            return new LoadingState<T>
            {
                IsSuccess = true,
                Value = value,
                LoadingFlag = code
            };
        }

        public static LoadingState<T> Fail(LoadingFlag code = LoadingFlag.Fail)
        {
            return new LoadingState<T>
            {
                IsSuccess = false,
                LoadingFlag = code
            };
        }

        public static LoadingState<T> Fail(string message, LoadingFlag code = LoadingFlag.Fail)
        {
            return new LoadingState<T>
            {
                IsSuccess = false,
                ErrorMessage = message,
                LoadingFlag = code
            };
        }
    }
}