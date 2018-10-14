using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;

class ObjectSaver
{

    /// <summary>
    /// Writes the given object instance to an XML file.
    /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
    /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [XmlIgnore] attribute.</para>
    /// <para>Object type must have a parameterless constructor.</para>
    /// <para>Object type must have a [Serializable] attribute</para>
    /// </summary>
    /// <typeparam name="T">The type of object being written to the file.</typeparam>
    /// <param name="filePath">The file path to write the object instance to.</param>
    /// <param name="objectToWrite">The object instance to write to the file.</param>
    /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
    public static void WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
    {
        TextWriter writer = null;
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            writer = new StreamWriter(filePath, append);

            serializer.Serialize(writer, objectToWrite);
        }
        finally
        {
            if (writer != null)
                writer.Close();
        }
    }

    /// <summary>
    /// Reads an object instance from an XML file.
    /// <para>Object type must have a parameterless constructor.</para>
    /// </summary>
    /// <typeparam name="T">The type of object to read from the file.</typeparam>
    /// <param name="filePath">The file path to read the object instance from.</param>
    /// <returns>Returns a new instance of the object read from the XML file.</returns>
    public static T ReadFromXmlFile<T>(string filePath) where T : new()
    {
        XmlReader reader = null;
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            //reader = new StreamReader(filePath, true);
            XmlReaderSettings settings = new XmlReaderSettings
            {
                CheckCharacters = false
            };
            reader = XmlReader.Create(filePath, settings);
            return (T)serializer.Deserialize(reader);
        }
        finally
        {
            if (reader != null)
                reader.Close();
        }
    }

    private byte[] ObjectToByteArray(object obj)
    {
        if (obj == null)
            return null;

        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, obj);

        return ms.ToArray();
    }
    private T ByteArrayToObject<T>(byte[] arrBytes)
    {
        MemoryStream memStream = new MemoryStream();
        BinaryFormatter binForm = new BinaryFormatter();
        memStream.Write(arrBytes, 0, arrBytes.Length);
        memStream.Seek(0, SeekOrigin.Begin);
        T obj = (T)binForm.Deserialize(memStream);

        return obj;
    }

    public static void SerializeObject<T>(string location, T obj)
    {
        IFormatter formatter = new BinaryFormatter();
        using (Stream stream = new FileStream(location, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            formatter.Serialize(stream, obj);
        }
    }
    public static T DeserializeObject<T>(string location)
    {
        IFormatter formatter = new BinaryFormatter();
        using (Stream stream = new FileStream(location, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            return (T)formatter.Deserialize(stream);
        }
    }
}