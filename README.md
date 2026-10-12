# Ais.Net.Models.Json

Low-allocation, UTF-8-first, polymorphic [System.Text.Json](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/) serialization for the [Ais.Net.Models](https://github.com/ais-dotnet/Ais.Net.Models) AIS message types.

Designed for broadcasting decoded AIS messages over byte-oriented transports such as [NATS](https://nats.io/) and reconstructing the concrete .NET type on receipt. Sponsored by [endjin](https://endjin.com).

| Package                                                                               | Status                                                                                                                                              |
|---------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------|
| [Ais.Net](https://github.com/ais-dotnet/Ais.Net)                                      | [![#](https://img.shields.io/nuget/v/Ais.Net.svg)](https://www.nuget.org/packages/Ais.Net/)                                                         |
| [Ais.Net.Models](https://github.com/ais-dotnet/Ais.Net.Models)                        | [![#](https://img.shields.io/nuget/v/Ais.Net.Models.svg)](https://www.nuget.org/packages/Ais.Net.Models/)                                           |
| [Ais.Net.Models.Json](https://github.com/ais-dotnet/Ais.Net.Models.Json)              | [![#](https://img.shields.io/nuget/v/Ais.Net.Models.Json.svg)](https://www.nuget.org/packages/Ais.Net.Models.Json/)                                 |
| [Ais.Net.Models.Json.Nats](https://github.com/ais-dotnet/Ais.Net.Models.Json.Nats)    | [![#](https://img.shields.io/nuget/v/Ais.Net.Models.Json.Nats.svg)](https://www.nuget.org/packages/Ais.Net.Models.Json.Nats/)                       |
| [Ais.Net.Receiver](https://github.com/ais-dotnet/Ais.Net.Receiver)                    | [![#](https://img.shields.io/nuget/v/Ais.Net.Receiver.svg)](https://www.nuget.org/packages/Ais.Net.Receiver/)                                       |
| [Ais.Net.Receiver.Storage.Azure.Blob](https://github.com/ais-dotnet/Ais.Net.Receiver) | [![#](https://img.shields.io/nuget/v/Ais.Net.Receiver.Storage.Azure.Blob.svg)](https://www.nuget.org/packages/Ais.Net.Receiver.Storage.Azure.Blob/) |

The [AIS.NET](https://github.com/ais-dotnet/) project contains a series of layers, from a [low-level zero allocation, high performance NMEA AIS sentence decoder](https://github.com/ais-dotnet/Ais.Net/), to [rich high-level C# 9.0 models](https://github.com/ais-dotnet/Ais.Net.Models/) of AIS message types, and a [receiver component](https://github.com/ais-dotnet/Ais.Net.Receiver/) that can listen to TCP streams of NMEA sentences and expose them as an `IObservable<string>` of raw sentences or a decoded `IObservable<IAisMessage>`. [`Ais.Net.Models.Json`](https://github.com/ais-dotnet/Ais.Net.Models.Json/) sits on top of `Ais.Net.Models` and turns the decoded messages into UTF-8 JSON and back, and `Ais.Net.Models.Json.Nats` is the top layer, adapting that serializer to NATS.Net so messages can be published and subscribed over NATS.

![https://github.com/ais-dotnet](https://endjincdn.blob.core.windows.net/assets/ais-dotnet-project-layers-incl-json.png)

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

For NATS, use the dedicated [`Ais.Net.Models.Json.Nats`](https://github.com/ais-dotnet/Ais.Net.Models.Json.Nats) package — a ready-made NATS.Net serializer and registry built on these primitives.

## Building

Requires the .NET 10 SDK. Depends on the `Ais.Net.Models` package (1.0.0 or later).

```
dotnet build Solutions/Ais.Net.Models.Json.slnx -c Release
```

## Licenses

Licensed under the [Apache 2.0 License](./LICENSE).


[![OpenChain](https://imm.endjin.com/api/imm/github/ais-dotnet/Ais.Net.Models.Json/rule/66efac1a-662c-40cf-b4ec-8b34c29e9fd7?cache=false)](https://imm.endjin.com/api/imm/github/ais-dotnet/Ais.Net.Models.Json/rule/66efac1a-662c-40cf-b4ec-8b34c29e9fd7?cache=false)

