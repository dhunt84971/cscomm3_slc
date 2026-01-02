// CSComm3.SLC - C# SLC PLC Communication Library
// Based on pycomm3 (https://github.com/ottowayi/pycomm3)

#if NETSTANDARD2_0
using System;
using System.Runtime.Serialization;
#endif

namespace CSComm3.SLC.Exceptions
{
    /// <summary>
    /// Base exception class for all CSComm3.SLC communication-related errors.
    /// </summary>
    [Serializable]
    public class CommunicationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationException"/> class.
        /// </summary>
        public CommunicationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public CommunicationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CommunicationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if NETSTANDARD2_0
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected CommunicationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }

    /// <summary>
    /// Exception thrown when there is a connection or socket-related error.
    /// </summary>
    [Serializable]
    public class CommException : CommunicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommException"/> class.
        /// </summary>
        public CommException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public CommException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CommException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if NETSTANDARD2_0
        /// <summary>
        /// Initializes a new instance of the <see cref="CommException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected CommException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }

    /// <summary>
    /// Exception thrown when there is an error encoding or decoding data.
    /// </summary>
    [Serializable]
    public class DataException : CommunicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataException"/> class.
        /// </summary>
        public DataException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public DataException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if NETSTANDARD2_0
        /// <summary>
        /// Initializes a new instance of the <see cref="DataException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected DataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }

    /// <summary>
    /// Exception thrown when trying to decode from an empty buffer.
    /// Used internally for array decoding to detect end of data.
    /// </summary>
    [Serializable]
    public class BufferEmptyException : DataException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BufferEmptyException"/> class.
        /// </summary>
        public BufferEmptyException()
            : base("Buffer is empty")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferEmptyException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public BufferEmptyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferEmptyException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public BufferEmptyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if NETSTANDARD2_0
        /// <summary>
        /// Initializes a new instance of the <see cref="BufferEmptyException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected BufferEmptyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }

    /// <summary>
    /// Exception thrown when there is an error in a response from the PLC.
    /// </summary>
    [Serializable]
    public class ResponseException : CommunicationException
    {
        /// <summary>
        /// Gets the status code from the response.
        /// </summary>
        public int? StatusCode { get; }

        /// <summary>
        /// Gets the extended status code from the response.
        /// </summary>
        public int? ExtendedStatus { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseException"/> class.
        /// </summary>
        public ResponseException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public ResponseException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseException"/> class with a specified error message
        /// and status codes.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="extendedStatus">The extended status code.</param>
        public ResponseException(string message, int statusCode, int? extendedStatus = null)
            : base(message)
        {
            StatusCode = statusCode;
            ExtendedStatus = extendedStatus;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if NETSTANDARD2_0
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected ResponseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            StatusCode = info.GetInt32(nameof(StatusCode));
            ExtendedStatus = info.GetInt32(nameof(ExtendedStatus));
        }

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(StatusCode), StatusCode);
            info.AddValue(nameof(ExtendedStatus), ExtendedStatus);
        }
#endif
    }

    /// <summary>
    /// Exception thrown when there is an error building a request.
    /// </summary>
    [Serializable]
    public class RequestException : CommunicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestException"/> class.
        /// </summary>
        public RequestException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public RequestException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public RequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if NETSTANDARD2_0
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected RequestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
