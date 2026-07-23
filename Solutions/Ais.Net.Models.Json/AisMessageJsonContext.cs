// <copyright file="AisMessageJsonContext.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

using Ais.Net.Models;
using Ais.Net.Models.Abstractions;

namespace Ais.Net.Models.Json;

// Source-generated, reflection-free JSON metadata for the AIS message types. Metadata mode is
// required because the messages are serialized polymorphically (the fast-path mode cannot do
// polymorphic dispatch). Nulls are omitted to keep payloads small; a null nullable round-trips
// back to null. The context is internal so the generator's public members are not part of this
// package's public API (and so they do not require XML doc comments).
[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Metadata,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(AisMessageBase))]
[JsonSerializable(typeof(AisMessageType1Through3))]
[JsonSerializable(typeof(AisMessageType5))]
[JsonSerializable(typeof(AisMessageType18))]
[JsonSerializable(typeof(AisMessageType19))]
[JsonSerializable(typeof(AisMessageType24Part0))]
[JsonSerializable(typeof(AisMessageType24Part1))]
[JsonSerializable(typeof(AisMessageType27))]
[JsonSerializable(typeof(Position))]
internal sealed partial class AisMessageJsonContext : JsonSerializerContext;
