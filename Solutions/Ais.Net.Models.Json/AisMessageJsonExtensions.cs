// <copyright file="AisMessageJsonExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Buffers;

namespace Ais.Net.Models.Json;

/// <summary>
/// Ergonomic extension members over <see cref="AisMessageJson"/> for serializing and deserializing
/// AIS messages as UTF-8 JSON.
/// </summary>
public static class AisMessageJsonExtensions
{
    extension(AisMessageBase message)
    {
        /// <summary>
        /// Serializes this message to a newly allocated UTF-8 JSON byte array.
        /// </summary>
        /// <returns>The UTF-8 JSON representation.</returns>
        public byte[] ToJsonUtf8Bytes() => AisMessageJson.SerializeToUtf8Bytes(message);

        /// <summary>
        /// Serializes this message as UTF-8 JSON directly into the supplied buffer writer.
        /// </summary>
        /// <param name="bufferWriter">The buffer writer to receive the UTF-8 JSON.</param>
        public void WriteJsonTo(IBufferWriter<byte> bufferWriter) => AisMessageJson.Serialize(bufferWriter, message);
    }

    extension(AisMessageBase)
    {
        /// <summary>
        /// Deserializes a UTF-8 JSON payload back into the concrete message type indicated by its
        /// <c>$type</c> discriminator.
        /// </summary>
        /// <param name="utf8Json">The UTF-8 JSON payload.</param>
        /// <returns>The deserialized message, or <see langword="null"/> when the payload is JSON null.</returns>
        public static AisMessageBase? FromJsonUtf8(ReadOnlySpan<byte> utf8Json) => AisMessageJson.Deserialize(utf8Json);
    }
}
