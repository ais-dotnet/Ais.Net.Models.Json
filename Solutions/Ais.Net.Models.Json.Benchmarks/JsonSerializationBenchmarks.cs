namespace Ais.Net.Models.Json.Benchmarks;

using System;
using System.Buffers;

using Ais.Net.Models;
using Ais.Net.Models.Abstractions;
using Ais.Net.Models.Json;

using BenchmarkDotNet.Attributes;

/// <summary>
/// Measures the JSON serialization paths: allocating byte array vs writing into a reused
/// <see cref="ArrayBufferWriter{T}"/> (the NATS-representative, lowest-allocation path), and
/// deserialization from a cached UTF-8 payload. Run across all seven AIS message types.
/// </summary>
[MemoryDiagnoser]
public class JsonSerializationBenchmarks
{
    private readonly ArrayBufferWriter<byte> bufferWriter = new(512);
    private AisMessageBase message = null!;
    private byte[] utf8Json = null!;
    private System.Text.Json.Utf8JsonWriter pooledWriter = null!;

    [Params("Type1Through3", "Type5", "Type18", "Type19", "Type24Part0", "Type24Part1", "Type27")]
    public string MessageKind { get; set; } = "Type1Through3";

    [GlobalSetup]
    public void Setup()
    {
        this.message = this.MessageKind switch
        {
            "Type1Through3" => CreateType1Through3(),
            "Type5" => CreateType5(),
            "Type18" => CreateType18(),
            "Type19" => CreateType19(),
            "Type24Part0" => CreateType24Part0(),
            "Type24Part1" => CreateType24Part1(),
            "Type27" => CreateType27(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(this.MessageKind), this.MessageKind, "Unknown message kind."),
        };
        this.utf8Json = AisMessageJson.SerializeToUtf8Bytes(this.message);
        this.pooledWriter = new System.Text.Json.Utf8JsonWriter(this.bufferWriter);
    }

    [GlobalCleanup]
    public void Cleanup() => this.pooledWriter.Dispose();

    [Benchmark]
    public byte[] SerializeToBytes() => AisMessageJson.SerializeToUtf8Bytes(this.message);

    [Benchmark]
    public int SerializeToBufferWriter()
    {
        this.bufferWriter.ResetWrittenCount();
        AisMessageJson.Serialize(this.bufferWriter, this.message);
        return this.bufferWriter.WrittenCount;
    }

    [Benchmark]
    public int SerializeToPooledWriter()
    {
        this.bufferWriter.ResetWrittenCount();
        this.pooledWriter.Reset(this.bufferWriter);
        AisMessageJson.Serialize(this.pooledWriter, this.message);
        this.pooledWriter.Flush();
        return this.bufferWriter.WrittenCount;
    }

    [Benchmark]
    public AisMessageBase? Deserialize() => AisMessageJson.Deserialize(this.utf8Json);

    private static AisMessageType1Through3 CreateType1Through3() => new(
        CourseOverGround: 123.4f,
        ManoeuvreIndicator: ManoeuvreIndicator.NotAvailable,
        MessageType: 1,
        Mmsi: 235009802,
        NavigationStatus: NavigationStatus.UnderwayUsingEngine,
        Position: new Position(50.12345, -1.98765),
        PositionAccuracy: true,
        RadioSlotTimeout: 1,
        RadioSubMessage: 2,
        RadioSyncState: RadioSyncState.UtcDirect,
        RateOfTurn: 0,
        RaimFlag: false,
        RepeatIndicator: 0,
        SpareBits145: 0,
        SpeedOverGround: 12.3f,
        TimeStampSecond: 42,
        TrueHeadingDegrees: 181);

    private static AisMessageType5 CreateType5() => new(
        AisVersion: 1,
        CallSign: "GBADG",
        Destination: "SOUTHAMPTON",
        DimensionToBow: 100,
        DimensionToPort: 15,
        DimensionToStarboard: 15,
        DimensionToStern: 100,
        Draught10thMetres: 75,
        EtaDay: 15,
        EtaHour: 12,
        EtaMinute: 30,
        EtaMonth: 6,
        IsDteNotReady: false,
        ImoNumber: 9074729,
        Mmsi: 235009802,
        PositionFixType: EpfdFixType.Gps,
        RepeatIndicator: 0,
        ShipType: (ShipType)70,
        Spare423: 0,
        VesselName: "EVER GIVEN");

    private static AisMessageType18 CreateType18() => new(
        CanAcceptMessage22ChannelAssignment: true,
        CanSwitchBands: true,
        CourseOverGround: 123.4f,
        CsUnit: ClassBUnit.Cstdma,
        HasDisplay: true,
        IsAssigned: false,
        IsDscAttached: true,
        Mmsi: 235009802,
        Position: new Position(50.12345, -1.98765),
        PositionAccuracy: true,
        RadioStatusType: ClassBRadioStatusType.Itdma,
        RaimFlag: false,
        RegionalReserved139: 0,
        RegionalReserved38: 0,
        RepeatIndicator: 0,
        SpeedOverGround: 12.3f,
        TimeStampSecond: 42,
        TrueHeadingDegrees: 181);

    private static AisMessageType19 CreateType19() => new(
        CourseOverGround: 123.4f,
        DimensionToBow: 100,
        DimensionToPort: 15,
        DimensionToStarboard: 15,
        DimensionToStern: 100,
        IsAssigned: false,
        IsDteNotReady: false,
        Mmsi: 235009802,
        Position: new Position(50.12345, -1.98765),
        PositionAccuracy: true,
        PositionFixType: EpfdFixType.Gps,
        RaimFlag: false,
        RegionalReserved139: 0,
        RegionalReserved38: 0,
        RepeatIndicator: 0,
        ShipName: "EVER GIVEN",
        ShipType: (ShipType)70,
        Spare308: 0,
        SpeedOverGround: 12.3f,
        TimeStampSecond: 42,
        TrueHeadingDegrees: 181);

    private static AisMessageType24Part0 CreateType24Part0() => new(
        Mmsi: 235009802,
        PartNumber: 0,
        RepeatIndicator: 0,
        Spare160: 0);

    private static AisMessageType24Part1 CreateType24Part1() => new(
        CallSign: "GBADG",
        DimensionToBow: 100,
        DimensionToPort: 15,
        DimensionToStarboard: 15,
        DimensionToStern: 100,
        Mmsi: 235009802,
        MothershipMmsi: 0,
        PartNumber: 1,
        RepeatIndicator: 0,
        SerialNumber: 12345,
        Spare162: 0,
        ShipType: (ShipType)70,
        UnitModelCode: 1,
        VendorIdRev3: "SRT",
        VendorIdRev4: "SRT");

    private static AisMessageType27 CreateType27() => new(
        CourseOverGround: 123.4f,
        GnssPositionStatus: true,
        Mmsi: 235009802,
        NavigationStatus: NavigationStatus.UnderwayUsingEngine,
        Position: new Position(50.12345, -1.98765),
        PositionAccuracy: true,
        RaimFlag: false,
        RepeatIndicator: 0,
        SpeedOverGround: 12.3f);
}
