// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

/// <summary>
/// A basic network package that can be sent through the <see cref="DataSender"/>.
/// </summary>
public class NetworkPackage
{
    public       string data;
    public       string dataType;
    public const int    MaxPackageSize = 1024;
    
    [JsonConstructor]
    public NetworkPackage(string data, string dataType)
    {
        this.dataType = dataType;
        this.data = data;
    }
    
    /// <summary>
    /// Creates a new network package with the given data.
    /// </summary>
    public static NetworkPackage CreatePackage<T>(T data) => new(JsonConvert.SerializeObject(data), typeof(T).ToString());
    
    /// <summary>
    /// Converts the data of this network package to the right type.
    /// If the conversion type does not match the type of the variable when the network package was created
    /// and error will be thrown.
    /// </summary>
    public T GetData<T>()
    {
        if (typeof(T).ToString() != dataType)
            throw new InvalidCastException($"Cannot convert {typeof(T)} to actual type {dataType}.");
        
        return JsonConvert.DeserializeObject<T>(data);
    }
}

