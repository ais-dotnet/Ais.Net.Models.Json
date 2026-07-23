// <copyright file="AisMessageJson.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using Ais.Net.Models;

namespace Ais.Net.Models.Json;

/// <summary>
/// UTF-8-first, source-generated JSON serialization for AIS messages, designed for low allocation
/// and for broadcasting over byte-oriented transports such as NATS.
/// </summary>
/// <remarks>
/// Messages round-trip polymorphically: any concrete <see cref="AisMessageBase"/> is written with a
/// <c>$type</c> discriminator as the first property and read back as the correct concrete type. The
/// discriminator tags are a permanent wire contract. The polymorphism is configured here rather than
/// with attributes on the model types, so the core <c>Ais.Net.Models</c> assembly has no dependency
/// on System.Text.Json.
/// </remarks>
public static class AisMessageJson
{
    private static readonly JsonSerializerOptions SerializerOptions = BuildOptions();

    private static readonly JsonTypeInfo<AisMessageBase> MessageTypeInfo =
        (JsonTypeInfo<AisMessageBase>)SerializerOptions.GetTypeInfo(typeof(AisMessageBase));

    /// <summary>
    /// Gets the configured serializer options (the source-generated resolver plus the polymorphism
    /// configuration). Exposed so transport integrations (for example a NATS serializer) can use
    /// System.Text.Json directly.
    /// </summary>
    public static JsonSerializerOptions Options => SerializerOptions;

    /// <summary>
    /// Gets the source-generated type metadata for the polymorphic message root, for low-allocation
    /// and transport-integration scenarios.
    /// </summary>
    public static JsonTypeInfo<AisMessageBase> TypeInfo => MessageTypeInfo;

    /// <summary>
    /// Serializes a message to a newly allocated UTF-8 JSON byte array.
    /// </summary>
    /// <param name="message">The message to serialize.</param>
    /// <returns>The UTF-8 JSON representation.</returns>
    public static byte[] SerializeToUtf8Bytes(AisMessageBase message) =>
        JsonSerializer.SerializeToUtf8Bytes(message, MessageTypeInfo);

    /// <summary>
    /// Serializes a message as UTF-8 JSON directly into a caller-provided buffer writer, without an
    /// intermediate array. This is the lowest-allocation path for byte-oriented transports.
    /// </summary>
    /// <param name="bufferWriter">The buffer writer to receive the UTF-8 JSON.</param>
    /// <param name="message">The message to serialize.</param>
    public static void Serialize(IBufferWriter<byte> bufferWriter, AisMessageBase message)
    {
        ArgumentNullException.ThrowIfNull(bufferWriter);

        using Utf8JsonWriter writer = new(bufferWriter);
        JsonSerializer.Serialize(writer, message, MessageTypeInfo);
    }

    /// <summary>
    /// Serializes a message using a caller-supplied <see cref="Utf8JsonWriter"/>. This is the
    /// lowest-allocation path for a hot broadcast loop: reuse a single writer across many messages
    /// via <see cref="Utf8JsonWriter.Reset(IBufferWriter{byte})"/>, avoiding a per-message writer
    /// allocation. The caller owns resetting and flushing the writer.
    /// </summary>
    /// <param name="writer">The writer to serialize into.</param>
    /// <param name="message">The message to serialize.</param>
    public static void Serialize(Utf8JsonWriter writer, AisMessageBase message)
    {
        ArgumentNullException.ThrowIfNull(writer);

        JsonSerializer.Serialize(writer, message, MessageTypeInfo);
    }

    /// <summary>
    /// Deserializes a UTF-8 JSON payload back into the concrete message type indicated by its
    /// <c>$type</c> discriminator.
    /// </summary>
    /// <param name="utf8Json">The UTF-8 JSON payload.</param>
    /// <returns>The deserialized message, or <see langword="null"/> when the payload is JSON null.</returns>
    public static AisMessageBase? Deserialize(ReadOnlySpan<byte> utf8Json) =>
        JsonSerializer.Deserialize(utf8Json, MessageTypeInfo);

    private static JsonSerializerOptions BuildOptions()
    {
        JsonSerializerOptions options = new()
        {
            TypeInfoResolver = AisMessageJsonContext.Default.WithAddedModifier(ConfigurePolymorphism),

            // Applied on the options actually used for (de)serialization. The matching setting on the
            // context's [JsonSourceGenerationOptions] only configures the context's own Options, which
            // this custom options instance does not use.
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        options.MakeReadOnly();
        return options;
    }

    // Attaches the polymorphic $type discriminator and derived-type tags to AisMessageBase without
    // any System.Text.Json attributes on the core model types. The tags are a permanent wire
    // contract (renaming one is a breaking protocol change). They disambiguate the two type 24 parts
    // (which share MessageType 24) and the type 1/2/3 range (which shares one record type).
    private static void ConfigurePolymorphism(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type != typeof(AisMessageBase))
        {
            return;
        }

        typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = "$type",
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
            DerivedTypes =
            {
                new JsonDerivedType(typeof(AisMessageType1Through3), "t1_3"),
                new JsonDerivedType(typeof(AisMessageType5), "t5"),
                new JsonDerivedType(typeof(AisMessageType18), "t18"),
                new JsonDerivedType(typeof(AisMessageType19), "t19"),
                new JsonDerivedType(typeof(AisMessageType24Part0), "t24p0"),
                new JsonDerivedType(typeof(AisMessageType24Part1), "t24p1"),
                new JsonDerivedType(typeof(AisMessageType27), "t27"),
            },
        };
    }
}
