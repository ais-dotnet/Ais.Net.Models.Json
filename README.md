# Ais.Net.Models.Json

Low-allocation, UTF-8-first, polymorphic [System.Text.Json](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/) serialization for the [Ais.Net.Models](https://github.com/ais-dotnet/Ais.Net.Models) AIS message types.

Designed for broadcasting decoded AIS messages over byte-oriented transports such as [NATS](https://nats.io/) and reconstructing the concrete .NET type on receipt. Sponsored by [endjin](https://endjin.com).

## Features

- **Source-generated** (reflection-free, trim/AOT-friendly) via `System.Text.Json`.
- **Polymorphic round-trip**: any `AisMessageBase` is written with a `$type` discriminator (the first property) and read back as the correct concrete message type.
- **UTF-8-first, low allocation**: serialize into an `IBufferWriter<byte>` or a reusable `Utf8JsonWriter`; deserialize directly from a `ReadOnlySpan<byte>`.
- Enums are serialized as numbers; null values are omitted.
- Ergonomic C# 14 extension members.
- No NATS dependency — the transport stays a consumer concern.

## Usage

```csharp
using Ais.Net.Models;
using Ais.Net.Models.Json;

// Serialize to UTF-8 JSON (with a $type discriminator).
byte[] utf8 = AisMessageJson.SerializeToUtf8Bytes(message);

// ...or, lower allocation, into a buffer writer or a reusable writer:
AisMessageJson.Serialize(bufferWriter, message);

// ...or via extension members:
byte[] utf8b = message.ToJsonUtf8Bytes();

// Deserialize back into the concrete message type.
AisMessageBase? received = AisMessageJson.Deserialize(utf8);
if (received is AisMessageType18 positionReport)
{
    // ...
}
```

### NATS

This library has no NATS dependency. Wire it up in your application with a small adapter over the primitives:

```csharp
using System.Buffers;
using Ais.Net.Models;
using Ais.Net.Models.Json;
using NATS.Client.Core;

public sealed class AisNatsSerializer
    : INatsSerialize<AisMessageBase>, INatsDeserialize<AisMessageBase>
{
    public static readonly AisNatsSerializer Default = new();

    public void Serialize(IBufferWriter<byte> bufferWriter, AisMessageBase value)
        => AisMessageJson.Serialize(bufferWriter, value);

    public AisMessageBase? Deserialize(in ReadOnlySequence<byte> buffer)
        => buffer.IsSingleSegment
            ? AisMessageJson.Deserialize(buffer.FirstSpan)
            : AisMessageJson.Deserialize(buffer.ToArray());

    public INatsSerializer<AisMessageBase> CombineWith(INatsSerializer<AisMessageBase> next) => this;
}
```

## Building

Requires the .NET 10 SDK. Depends on the `Ais.Net.Models` package (1.0.0 or later).

```
dotnet build Solutions/Ais.Net.Models.Json.slnx -c Release
```

## Licenses

Licensed under the [Apache 2.0 License](./LICENSE).


[![OpenChain](https://imm.endjin.com/api/imm/github/ais-dotnet/Ais.Net.Models.Json/rule/66efac1a-662c-40cf-b4ec-8b34c29e9fd7?cache=false)](https://imm.endjin.com/api/imm/github/ais-dotnet/Ais.Net.Models.Json/rule/66efac1a-662c-40cf-b4ec-8b34c29e9fd7?cache=false)

