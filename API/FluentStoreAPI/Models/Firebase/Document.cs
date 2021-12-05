using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
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

            JToken fieldToken = field[fieldType];
            switch (fieldType.Replace("Value", ""))
            {
                case "boolean":
                    fieldValue = fieldToken.ToObject<bool>();
                    break;
                case "string":
                    fieldValue = fieldToken.ToObject<string>();
                    break;
                case "number":
                    fieldValue = fieldToken.ToObject<double>();
                    break;
                case "map":
                    fieldValue = fieldToken["fields"];
                    break;
                case "array":
                    var jarray = fieldToken["values"];
                    if (jarray != null && jarray.HasValues)
                        fieldValue = jarray.Select(item => TransformField(item)).ToList();
                    else
                        fieldValue = null;
                    break;
                case "timestamp":
                    fieldValue = fieldToken.ToObject<DateTimeOffset>();
                    break;
                case "null":
                    fieldValue = null;
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
                    if (fieldValue != null && fieldValue.GetType().IsGenericType)
                    {
                        // Ignore generics, since TransformField will set
                        // all type parameters to object
                        var setter = tType.GetMethod("Set" + fieldName);
                        setter.Invoke(result, new[] { fieldValue });
                    }
                    else
                    {
                        PropertyInfo targetProp = tType.GetProperty(fieldName, fieldValue?.GetType() ?? typeof(object));
                        if (targetProp != null && targetProp.CanWrite && !targetProp.IsDefined(typeof(IgnoreAttribute)))
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
            PropertyInfo createdAtProp = tType.GetProperty("CreatedAt", typeof(DateTimeOffset));
            if (createdAtProp != null)
                createdAtProp.SetValue(result, CreatedAt);

            // Set UpdatedAt
            PropertyInfo updatedAtProp = tType.GetProperty("UpdatedAt", typeof(DateTimeOffset));
            if (updatedAtProp != null)
                updatedAtProp.SetValue(result, UpdatedAt);

            return result;
        }

        public static JObject UntransformField(object value)
        {
            var jObj = new JObject();
            if (value == null)
            {
                jObj.Add("nullValue", null);
                return jObj;
            }

            Type type = value.GetType();
            if (type.IsAssignableFrom(typeof(bool)))
            {
                jObj.Add("booleanValue", JToken.FromObject(value));
            }
            else if (type.IsAssignableFrom(typeof(string)))
            {
                jObj.Add("stringValue", JToken.FromObject(value));
            }
            else if (type.IsAssignableFrom(typeof(int)) || type.IsAssignableFrom(typeof(double)))
            {
                jObj.Add("numberValue", JToken.FromObject(value));
            }
            else if (type.Name.StartsWith("Array") || type.Name.StartsWith("List") || type.Name.StartsWith("IList") || type.Name.StartsWith("IEnumerable"))
            {
                var items = new List<JObject>();
                foreach (object obj in value as IEnumerable)
                    items.Add(UntransformField(obj));
                var jValuesNode = new JObject();
                jValuesNode.Add("values", new JArray(items));
                jObj.Add("arrayValue", jValuesNode);
            }
            else if (type.IsAssignableFrom(typeof(DateTimeOffset)))
            {
                jObj.Add("timestampValue", JToken.FromObject(value));
            }

            return jObj;
        }

        public static Document Untransform(object source)
        {
            Document doc = new Document
            {
                Fields = new Dictionary<string, JObject>()
            };
            Type sType = source.GetType();
            foreach (PropertyInfo prop in sType.GetProperties())
            {
                if (prop.Name == "Id" || prop.Name == "CreatedAt" || prop.Name == "UpdatedAt"
                    || prop.IsDefined(typeof(IgnoreAttribute)))
                    continue;

                JObject jField = UntransformField(prop.GetValue(source));
                doc.Fields.Add(prop.Name, jField);
            }

            // Set document ID
            PropertyInfo idProp = sType.GetProperty("Id", typeof(Guid));
            if (idProp != null)
                doc.Name = ((Guid)idProp.GetValue(source)).ToString();

            // Set CreatedAt
            PropertyInfo createdAtProp = sType.GetProperty("CreatedAt", typeof(DateTimeOffset));
            if (createdAtProp != null)
                doc.CreatedAt = (DateTimeOffset)createdAtProp.GetValue(source);

            // Set UpdatedAt
            PropertyInfo updatedAtProp = sType.GetProperty("UpdatedAt", typeof(DateTimeOffset));
            if (updatedAtProp != null)
                doc.UpdatedAt = (DateTimeOffset)updatedAtProp.GetValue(source);

            return doc;
        }
    }
}
