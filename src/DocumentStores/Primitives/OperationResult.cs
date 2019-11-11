using System.Collections.Generic;

namespace DocumentStores.Primitives
{
    /// <summary>
    /// Represents the result of a performed operation.
    /// </summary>
    public class OperationResult
    {

        public OperationResult(bool success = true, string message = "", IEnumerable<OperationResult> innerResults = null)
        {
            this.Success = success;
            this.Message = message;
            this.InnerResults = innerResults ?? new OperationResult[] { };
        }

        public bool Success { get; }

        public string Message { get; }

        public IEnumerable<OperationResult> InnerResults { get; }

        public OperationResult<TData> SetData<TData>(TData data = default) =>
            new OperationResult<TData>(this.Success, this.Message, this.InnerResults, data);

        public static implicit operator OperationResult(bool success) =>
            new OperationResult(success, "", null);

        public static implicit operator OperationResult(string message) =>
            new OperationResult(false, message, null);

    }

    /// <summary>
    /// Represents the result of a performed operation, with custom return data.
    /// </summary>
    public class OperationResult<TData> : OperationResult
    {
        public OperationResult(bool success = true, string message = "", IEnumerable<OperationResult> innerResults = null, TData data = default) :
            base(success, message, innerResults) =>
            this.Data = data;

        public TData Data { get; }

        public static implicit operator OperationResult<TData>(TData data) =>
            new OperationResult<TData>(true, "", null, data);

        public static implicit operator TData(OperationResult<TData> result) =>
            result.Data;

        public static implicit operator OperationResult<TData>(bool success) =>
            new OperationResult<TData>(success, "", null);

        public static implicit operator OperationResult<TData>(string message) =>
            new OperationResult<TData>(false, message, null);

    }
}