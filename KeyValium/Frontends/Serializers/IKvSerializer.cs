using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace KeyValium.Frontends.Serializers
{
    /// <summary>
    /// Interface for serializers
    /// </summary>
    public interface IKvSerializer
    {
        /// <summary>
        /// The serializer options used. Stored in dictionary info at the time of creation.
        /// Can return null if no options exist.
        /// </summary>
        object Options
        {
            get;
        }

        /// <summary>
        /// Callback for setting dictionary options. It is called with the options
        /// </summary>
        /// <param name="options">The options to use for the serializer. This is the value returned by Options at the time of creation.</param>
        void SetOptions(JsonElement options);

        /// <summary>
        /// Callback to serialize an object of type T.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">the object to serialize</param>
        /// <param name="isvalue">If false a key is serialized. If true a value is serialized.</param>
        /// <returns>The serialized object as byte array.</returns>
        byte[] Serialize<T>(T data, bool isvalue);

        /// <summary>
        /// Callback to deserializes an object of type T.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">the byte array to deserialize</param>
        /// <param name="isvalue">If false a key is deserialized. If true a value is deserialized.</param>
        /// <returns>The deserialized object.</returns>
        T Deserialize<T>(ReadOnlySpan<byte> data, bool isvalue);
    }
}
