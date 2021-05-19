using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluentStoreAPI.Models.Firebase
{
    public class Document
    {
        /// <summary>
        /// A URL of the document. URL base is "https://firestore.googleapis.com/v1"
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The date and time the document was created at.
        /// </summary>
        [JsonProperty("createTime")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// The date and time the document was updated at.
        /// </summary>
        [JsonProperty("updateTime")]
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// The fields of this document.
        /// </summary>
        [JsonProperty("fields")]
        public Dictionary<string, JObject> Fields { get; set; }

        /// <summary>
        /// Attempts to create a .NET object from the specified field
        /// </summary>
        public object TransformField(JToken field)
        {
            object fieldValue = null;
            string fieldType = field.First.Path;
            int idxDot = fieldType.LastIndexOf('.');
            if (idxDot >= 0)
            {
                fieldType = fieldType.Split(new[] { '.' }).Last();
            }

            switch (fieldType.Replace("Value", ""))
            {
                case "boolean":
                    fieldValue = field["booleanValue"].ToObject<bool>();
                    break;
                case "string":
                    fieldValue = field["stringValue"].ToObject<string>();
                    break;
                case "number":
                    fieldValue = field["numberValue"].ToObject<double>();
                    break;
                case "map":
                    fieldValue = field["mapValue"]["fields"];
                    break;
                case "array":
                    fieldValue = field["arrayValue"]["values"].Select(item => TransformField(item)).ToList();
                    break;
                case "timestamp":
                    fieldValue = field["timestampValue"].ToObject<DateTimeOffset>();
                    break;

                case "geopoint":
                case "reference":
                default:
                    throw new NotImplementedException();
            }

            return fieldValue;
        }

        /// <summary>
        /// Attempts to map the current document to the model specified by <typeparamref name="T"/>.
        /// </summary>
        public T Transform<T>() where T : new()
        {
            T result = new T();
            Type tType = result.GetType();
            if (Fields != null)
            {
                foreach (string fieldName in Fields.Keys)
                {
                    object fieldValue = TransformField(Fields[fieldName]);
                    if (fieldValue.GetType().IsGenericType)
                    {
                        // Ignore generics, since TransformField will set
                        // all type parameters to object
                        var setter = tType.GetMethod("Set" + fieldName);
                        setter.Invoke(result, new[] { fieldValue });
                    }
                    else
                    {
                        PropertyInfo targetProp = tType.GetProperty(fieldName, fieldValue.GetType());
                        if (targetProp != null)
                            targetProp.SetValue(result, fieldValue);
                    }
                }
            }

            // Set document ID
            PropertyInfo idProp = tType.GetProperty("Id", typeof(Guid));
            if (idProp != null)
            {
                string id = Name.Split(new[] { '/' }).Last();
                idProp.SetValue(result, new Guid(id));
            }

            // Set CreatedAt
            PropertyInfo createdAtProp = tType.GetProperty("CreatedAt", typeof(Guid));
            if (createdAtProp != null)
                createdAtProp.SetValue(result, CreatedAt);

            // Set UpdatedAt
            PropertyInfo updatedAtProp = tType.GetProperty("UpdatedAt", typeof(Guid));
            if (updatedAtProp != null)
                updatedAtProp.SetValue(result, UpdatedAt);

            return result;
        }
    }
}
