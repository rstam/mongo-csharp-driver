/* Copyright 2010-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Conventions;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for objects.
    /// </summary>
    public class ObjectSerializer : ClassSerializerBase<object>
    {
        #region static
        // private static fields
        private static readonly Func<Type, bool> __allTypesAllowed;
        private static readonly Func<Type, bool> __defaultAllowedTypes;
        private static readonly IDiscriminatorConvention __defaultDiscriminatorConvention;
        private static readonly  GuidRepresentation __defaultGuidRepresentation;
        private static readonly ObjectSerializer __instance;
        private static readonly Func<Type, bool> __noTypesAllowed;

        static ObjectSerializer()
        {
            // use a static constructor to control the order of initializations
            __allTypesAllowed = t => true;
            __noTypesAllowed = t => false;
            __defaultAllowedTypes = __allTypesAllowed; ;
            __defaultGuidRepresentation = GuidRepresentation.Unspecified;
            __defaultDiscriminatorConvention = BsonSerializer.LookupDiscriminatorConvention(typeof(object));
            __instance = new ObjectSerializer();
        }

        /// <summary>
        /// An allowed types function that returns true for all types.
        /// </summary>
        public static Func<Type, bool> AllTypesAllowed => __allTypesAllowed;

        /// <summary>
        /// An allowed types function that returns false for all types.
        /// </summary>
        public static Func<Type, bool> NoTypesAllowed => __noTypesAllowed;
        #endregion

        // private fields
        private readonly Func<Type, bool> _allowedTypes;
        private readonly IDiscriminatorConvention _discriminatorConvention;
        private readonly GuidRepresentation _guidRepresentation;
        private readonly GuidSerializer _guidSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectSerializer"/> class.
        /// </summary>
        public ObjectSerializer()
            : this(__defaultDiscriminatorConvention)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectSerializer"/> class.
        /// </summary>
        /// <param name="discriminatorConvention">The discriminator convention.</param>
        /// <exception cref="System.ArgumentNullException">discriminatorConvention</exception>
        public ObjectSerializer(IDiscriminatorConvention discriminatorConvention)
            : this(discriminatorConvention, __defaultGuidRepresentation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectSerializer"/> class.
        /// </summary>
        /// <param name="discriminatorConvention">The discriminator convention.</param>
        /// <param name="guidRepresentation">The Guid representation.</param>
        public ObjectSerializer(IDiscriminatorConvention discriminatorConvention, GuidRepresentation guidRepresentation)
            : this(discriminatorConvention, guidRepresentation, __defaultAllowedTypes)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectSerializer"/> class.
        /// </summary>
        /// <param name="allowedTypes">A delegate that determines what types are allowed.</param>
        public ObjectSerializer(Func<Type, bool> allowedTypes)
            : this(__defaultDiscriminatorConvention, allowedTypes)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectSerializer"/> class.
        /// </summary>
        /// <param name="discriminatorConvention">The discriminator convention.</param>
        /// <param name="allowedTypes">A delegate that determines what types are allowed.</param>
        public ObjectSerializer(IDiscriminatorConvention discriminatorConvention, Func<Type, bool> allowedTypes)
            : this(discriminatorConvention, __defaultGuidRepresentation, allowedTypes)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectSerializer"/> class.
        /// </summary>
        /// <param name="discriminatorConvention">The discriminator convention.</param>
        /// <param name="guidRepresentation">The Guid representation.</param>
        /// <param name="allowedTypes">A delegate that determines what types are allowed.</param>
        public ObjectSerializer(IDiscriminatorConvention discriminatorConvention, GuidRepresentation guidRepresentation, Func<Type, bool> allowedTypes)
        {
            if (discriminatorConvention == null)
            {
                throw new ArgumentNullException("discriminatorConvention");
            }
            if (allowedTypes == null)
            {
                throw new ArgumentNullException(nameof(allowedTypes));
            }

            _discriminatorConvention = discriminatorConvention;
            _guidRepresentation = guidRepresentation;
            _guidSerializer = new GuidSerializer(_guidRepresentation);
            _allowedTypes = allowedTypes;
        }

        // public static properties
        /// <summary>
        /// Gets the standard instance.
        /// </summary>
        /// <value>
        /// The standard instance.
        /// </value>
        public static ObjectSerializer Instance
        {
            get { return __instance; }
        }

        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        public override object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Array:
                    if (context.DynamicArraySerializer != null)
                    {
                        return context.DynamicArraySerializer.Deserialize(context);
                    }
                    goto default;

                case BsonType.Binary:
#pragma warning disable 618
                    if (BsonDefaults.GuidRepresentationMode == GuidRepresentationMode.V2 && _guidRepresentation == GuidRepresentation.Unspecified)
                    {
                        var binaryData = bsonReader.ReadBinaryData();
                        var subType = binaryData.SubType;
                        if (subType == BsonBinarySubType.UuidStandard || subType == BsonBinarySubType.UuidLegacy)
                        {
                            return binaryData.ToGuid();
                        }
                    }
                    else
                    {
                        var binaryDataBookmark = bsonReader.GetBookmark();
                        var binaryData = bsonReader.ReadBinaryDataWithGuidRepresentationUnspecified();
                        var subType = binaryData.SubType;
                        if (subType == BsonBinarySubType.UuidStandard || subType == BsonBinarySubType.UuidLegacy)
                        {
                            bsonReader.ReturnToBookmark(binaryDataBookmark);
                            return _guidSerializer.Deserialize(context);
                        }
                    }
#pragma warning restore 618
                    goto default;

                case BsonType.Boolean:
                    return bsonReader.ReadBoolean();

                case BsonType.DateTime:
                    var millisecondsSinceEpoch = bsonReader.ReadDateTime();
                    var bsonDateTime = new BsonDateTime(millisecondsSinceEpoch);
                    return bsonDateTime.ToUniversalTime();

                case BsonType.Decimal128:
                    return bsonReader.ReadDecimal128();

                case BsonType.Document:
                    return DeserializeDiscriminatedValue(context, args);

                case BsonType.Double:
                    return bsonReader.ReadDouble();

                case BsonType.Int32:
                    return bsonReader.ReadInt32();

                case BsonType.Int64:
                    return bsonReader.ReadInt64();

                case BsonType.Null:
                    bsonReader.ReadNull();
                    return null;

                case BsonType.ObjectId:
                    return bsonReader.ReadObjectId();

                case BsonType.String:
                    return bsonReader.ReadString();

                default:
                    var message = string.Format("ObjectSerializer does not support BSON type '{0}'.", bsonType);
                    throw new FormatException(message);
            }
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            var bsonWriter = context.Writer;

            if (value == null)
            {
                bsonWriter.WriteNull();
            }
            else
            {
                var actualType = value.GetType();
                if (actualType == typeof(object))
                {
                    bsonWriter.WriteStartDocument();
                    bsonWriter.WriteEndDocument();
                }
                else
                {
                    // certain types can be written directly as BSON value
                    // if we're not at the top level document, or if we're using the JsonWriter
                    if (bsonWriter.State == BsonWriterState.Value || bsonWriter is JsonWriter)
                    {
                        switch (Type.GetTypeCode(actualType))
                        {
                            case TypeCode.Boolean:
                                bsonWriter.WriteBoolean((bool)value);
                                return;

                            case TypeCode.DateTime:
                                // TODO: is this right? will lose precision after round trip
                                var bsonDateTime = new BsonDateTime(BsonUtils.ToUniversalTime((DateTime)value));
                                bsonWriter.WriteDateTime(bsonDateTime.MillisecondsSinceEpoch);
                                return;

                            case TypeCode.Double:
                                bsonWriter.WriteDouble((double)value);
                                return;

                            case TypeCode.Int16:
                                // TODO: is this right? will change type to Int32 after round trip
                                bsonWriter.WriteInt32((short)value);
                                return;

                            case TypeCode.Int32:
                                bsonWriter.WriteInt32((int)value);
                                return;

                            case TypeCode.Int64:
                                bsonWriter.WriteInt64((long)value);
                                return;

                            case TypeCode.Object:
                                if (actualType == typeof(Decimal128))
                                {
                                    var decimal128 = (Decimal128)value;
                                    bsonWriter.WriteDecimal128(decimal128);
                                    return;
                                }
                                if (actualType == typeof(Guid))
                                {
                                    var guid = (Guid)value;
#pragma warning disable 618
                                    if (BsonDefaults.GuidRepresentationMode == GuidRepresentationMode.V2 && _guidRepresentation == GuidRepresentation.Unspecified)
                                    {
                                        var guidRepresentation = bsonWriter.Settings.GuidRepresentation;
                                        var binaryData = new BsonBinaryData(guid, guidRepresentation);
                                        bsonWriter.WriteBinaryData(binaryData);
                                    }
                                    else
                                    {
                                        _guidSerializer.Serialize(context, args, guid);
                                    }
#pragma warning restore 618
                                    return;
                                }
                                if (actualType == typeof(ObjectId))
                                {
                                    bsonWriter.WriteObjectId((ObjectId)value);
                                    return;
                                }
                                break;

                            case TypeCode.String:
                                bsonWriter.WriteString((string)value);
                                return;
                        }
                    }

                    SerializeDiscriminatedValue(context, args, value, actualType);
                }
            }
        }

        // private methods
        private object DeserializeDiscriminatedValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            var actualType = _discriminatorConvention.GetActualType(bsonReader, typeof(object));
            if (!_allowedTypes(actualType))
            {
                throw new BsonSerializationException($"Type {actualType.FullName} is not configured as an allowed type for this instance of ObjectSerializer.");
            }

            if (actualType == typeof(object))
            {
                var type = bsonReader.GetCurrentBsonType();
                switch (type)
                {
                    case BsonType.Document:
                        if (context.DynamicDocumentSerializer != null)
                        {
                            return context.DynamicDocumentSerializer.Deserialize(context, args);
                        }
                        break;
                }

                bsonReader.ReadStartDocument();
                bsonReader.ReadEndDocument();
                return new object();
            }
            else
            {
                var serializer = BsonSerializer.LookupSerializer(actualType);
                var polymorphicSerializer = serializer as IBsonPolymorphicSerializer;
                if (polymorphicSerializer != null && polymorphicSerializer.IsDiscriminatorCompatibleWithObjectSerializer)
                {
                    return serializer.Deserialize(context, args);
                }
                else
                {
                    object value = null;
                    var wasValuePresent = false;

                    bsonReader.ReadStartDocument();
                    while (bsonReader.ReadBsonType() != 0)
                    {
                        var name = bsonReader.ReadName();
                        if (name == _discriminatorConvention.ElementName)
                        {
                            bsonReader.SkipValue();
                        }
                        else if (name == "_v")
                        {
                            value = serializer.Deserialize(context);
                            wasValuePresent = true;
                        }
                        else
                        {
                            var message = string.Format("Unexpected element name: '{0}'.", name);
                            throw new FormatException(message);
                        }
                    }
                    bsonReader.ReadEndDocument();

                    if (!wasValuePresent)
                    {
                        throw new FormatException("_v element missing.");
                    }

                    return value;
                }
            }
        }

        private void SerializeDiscriminatedValue(BsonSerializationContext context, BsonSerializationArgs args, object value, Type actualType)
        {
            if (!_allowedTypes(actualType))
            {
                throw new BsonSerializationException($"Type {actualType.FullName} is not configured as an allowed type for this instance of ObjectSerializer.");
            }

            var serializer = BsonSerializer.LookupSerializer(actualType);

            var polymorphicSerializer = serializer as IBsonPolymorphicSerializer;
            if (polymorphicSerializer != null && polymorphicSerializer.IsDiscriminatorCompatibleWithObjectSerializer)
            {
                serializer.Serialize(context, args, value);
            }
            else
            {
                if (context.IsDynamicType != null && context.IsDynamicType(value.GetType()))
                {
                    args.NominalType = actualType;
                    serializer.Serialize(context, args, value);
                }
                else
                {
                    var bsonWriter = context.Writer;
                    var discriminator = _discriminatorConvention.GetDiscriminator(typeof(object), actualType);

                    bsonWriter.WriteStartDocument();
                    bsonWriter.WriteName(_discriminatorConvention.ElementName);
                    BsonValueSerializer.Instance.Serialize(context, discriminator);
                    bsonWriter.WriteName("_v");
                    serializer.Serialize(context, value);
                    bsonWriter.WriteEndDocument();
                }
            }
        }
    }
}
