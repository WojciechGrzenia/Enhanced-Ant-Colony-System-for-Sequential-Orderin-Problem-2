using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Controllers
{
    /// <summary>
    ///     Serializes data.
    /// </summary>
    public static class Serializer
    {
        static readonly object _serializeLock = new object();
        static readonly object _deserializeLock = new object();

        /// <summary>
        ///     Serializes the object.
        /// </summary>
        /// <param name="value">The object.</param>
        /// <returns>Object as binary data.</returns>
        public static byte[] SerializeObj(object value)
        {
            lock (_serializeLock)
            {
                using (var stream = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, value);
                    var bytes = stream.ToArray();
                    stream.Flush();

                    return bytes;
                }
            }
        }

        /// <summary>
        ///     Deserializes the object.
        /// </summary>
        /// <param name="binaryObj">The binary object.</param>
        /// <returns>Deserialized object.</returns>
        public static object DeserializeObj(byte[] binaryObj)
        {
            lock (_deserializeLock)
            {
                using (var stream = new MemoryStream(binaryObj))
                {
                    stream.Position = 0;
                    var formatter = new BinaryFormatter();
                    return formatter.Deserialize(stream);
                }
            }
        }
    }
}