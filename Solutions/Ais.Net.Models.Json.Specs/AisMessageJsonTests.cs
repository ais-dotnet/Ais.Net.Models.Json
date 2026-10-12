
using System.Buffers;
using System.Text;
using Ais.Net.Models.Abstractions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Shouldly;

namespace Ais.Net.Models.Json.Specs;
[TestClass]
public class AisMessageJsonTests
{
    [TestMethod]
    public void Type1Through3RoundTrips() => AssertRoundTrips(CreateType1Through3(messageType: 1));

    [TestMethod]
    public void Type5RoundTrips() => AssertRoundTrips(CreateType5());

    [TestMethod]
    public void Type18RoundTrips() => AssertRoundTrips(CreateType18());

    [TestMethod]
    public void Type19RoundTrips() => AssertRoundTrips(CreateType19());

    [TestMethod]
    public void Type24Part0RoundTrips() => AssertRoundTrips(CreateType24Part0());

    [TestMethod]
    public void Type24Part1RoundTrips() => AssertRoundTrips(CreateType24Part1());

    [TestMethod]
    public void Type27RoundTrips() => AssertRoundTrips(CreateType27());

    [TestMethod]
    public void NullPositionIsOmittedAndRoundTripsToNull()
    {
        AisMessageType18 message = CreateType18() with
        {
            Position = null,
            CourseOverGround = null,
            SpeedOverGround = null,
        };

        string json = Encoding.UTF8.GetString(AisMessageJson.SerializeToUtf8Bytes(message));

        // Match the exact property keys (a non-null "PositionAccuracy" also contains "Position").
        json.ShouldNotContain("\"Position\":");
        json.ShouldNotContain("\"CourseOverGround\":");

        AisMessageBase? result = AisMessageJson.Deserialize(AisMessageJson.SerializeToUtf8Bytes(message));
        result.ShouldBe(message);
    }

    [TestMethod]
    [DataRow("t1_3")]
    [DataRow("t5")]
    [DataRow("t18")]
    [DataRow("t19")]
    [DataRow("t24p0")]
    [DataRow("t24p1")]
    [DataRow("t27")]
    public void DiscriminatorIsTheFirstPropertyWithTheExpectedTag(string expectedTag)
    {
        AisMessageBase message = expectedTag switch
        {
            "t1_3" => CreateType1Through3(messageType: 1),
            "t5" => CreateType5(),
            "t18" => CreateType18(),
            "t19" => CreateType19(),
            "t24p0" => CreateType24Part0(),
            "t24p1" => CreateType24Part1(),
            _ => CreateType27(),
        };

        string json = Encoding.UTF8.GetString(AisMessageJson.SerializeToUtf8Bytes(message));

        json.ShouldStartWith($"{{\"$type\":\"{expectedTag}\"");
    }

    [TestMethod]
    public void Type24PartsAreDisambiguatedByTagAndDeserialiseToTheCorrectType()
    {
        // Both parts carry MessageType 24, so only the $type tag can distinguish them.
        AisMessageJson.Deserialize(AisMessageJson.SerializeToUtf8Bytes(CreateType24Part0()))
            .ShouldBeOfType<AisMessageType24Part0>();
        AisMessageJson.Deserialize(AisMessageJson.SerializeToUtf8Bytes(CreateType24Part1()))
            .ShouldBeOfType<AisMessageType24Part1>();
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    public void Type1Through3RoundTripsAcrossItsMessageTypes(int messageType)
    {
        AisMessageType1Through3 message = CreateType1Through3(messageType);

        byte[] json = AisMessageJson.SerializeToUtf8Bytes(message);
        Encoding.UTF8.GetString(json).ShouldStartWith("{\"$type\":\"t1_3\"");

        AisMessageBase? result = AisMessageJson.Deserialize(json);
        result.ShouldBe(message);
        result.ShouldBeOfType<AisMessageType1Through3>().MessageType.ShouldBe(messageType);
    }

    [TestMethod]
    public void SerializingViaTheBaseTypeStillWritesTheDerivedProperties()
    {
        AisMessageBase message = CreateType18();

        string json = Encoding.UTF8.GetString(AisMessageJson.SerializeToUtf8Bytes(message));

        // A type-18-only property must be present, proving polymorphic (not base-only) serialization.
        json.ShouldContain("CanSwitchBands");
    }

    [TestMethod]
    public void BufferWriterOutputMatchesTheByteArrayOutput()
    {
        AisMessageType18 message = CreateType18();

        byte[] viaBytes = AisMessageJson.SerializeToUtf8Bytes(message);

        ArrayBufferWriter<byte> writer = new();
        AisMessageJson.Serialize(writer, message);

        writer.WrittenSpan.ToArray().ShouldBe(viaBytes);
    }

    [TestMethod]
    public void PooledWriterOutputMatchesTheByteArrayOutput()
    {
        AisMessageType18 message = CreateType18();

        byte[] viaBytes = AisMessageJson.SerializeToUtf8Bytes(message);

        ArrayBufferWriter<byte> writerTarget = new();
        using System.Text.Json.Utf8JsonWriter writer = new(writerTarget);
        AisMessageJson.Serialize(writer, message);
        writer.Flush();

        writerTarget.WrittenSpan.ToArray().ShouldBe(viaBytes);
    }

    [TestMethod]
    public void EnumsAreSerialisedAsNumbers()
    {
        // ShipType is constructed as (ShipType)60, so its numeric wire value is 60.
        string json = Encoding.UTF8.GetString(AisMessageJson.SerializeToUtf8Bytes(CreateType5()));

        json.ShouldContain("\"ShipType\":60");
    }

    [TestMethod]
    public void ExtensionMembersMatchTheStaticApi()
    {
        AisMessageType18 message = CreateType18();

        byte[] viaStatic = AisMessageJson.SerializeToUtf8Bytes(message);
        byte[] viaExtension = message.ToJsonUtf8Bytes();
        viaExtension.ShouldBe(viaStatic);

        ArrayBufferWriter<byte> writer = new();
        message.WriteJsonTo(writer);
        writer.WrittenSpan.ToArray().ShouldBe(viaStatic);

        AisMessageBase.FromJsonUtf8(viaExtension).ShouldBe(message);
    }

    private static void AssertRoundTrips<T>(T original)
        where T : AisMessageBase
    {
        byte[] utf8Json = AisMessageJson.SerializeToUtf8Bytes(original);

        AisMessageBase? result = AisMessageJson.Deserialize(utf8Json);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<T>();
        result.ShouldBe(original);
    }

    private static AisMessageType1Through3 CreateType1Through3(int messageType) => new(
        CourseOverGround: 123.45f,
        ManoeuvreIndicator: ManoeuvreIndicator.NotAvailable,
        MessageType: messageType,
        Mmsi: 12345,
        NavigationStatus: NavigationStatus.UnderwayUsingEngine,
        Position: new Position(1.0, 2.0),
        PositionAccuracy: true,
        RadioSlotTimeout: 1,
        RadioSubMessage: 2,
        RadioSyncState: RadioSyncState.UtcDirect,
        RateOfTurn: 1,
        RaimFlag: true,
        RepeatIndicator: 3,
        SpareBits145: 4,
        SpeedOverGround: 12.34f,
        TimeStampSecond: 56,
        TrueHeadingDegrees: 78);

    private static AisMessageType5 CreateType5() => new(
        AisVersion: 1,
        CallSign: "CALL",
        Destination: "DEST",
        DimensionToBow: 1,
        DimensionToPort: 2,
        DimensionToStarboard: 3,
        DimensionToStern: 4,
        Draught10thMetres: 5,
        EtaDay: 6,
        EtaHour: 7,
        EtaMinute: 8,
        EtaMonth: 9,
        IsDteNotReady: true,
        ImoNumber: 123,
        Mmsi: 12345,
        PositionFixType: EpfdFixType.Gps,
        RepeatIndicator: 3,
        ShipType: (ShipType)60,
        Spare423: 1,
        VesselName: "VESSEL");

    private static AisMessageType18 CreateType18() => new(
        CanAcceptMessage22ChannelAssignment: true,
        CanSwitchBands: true,
        CourseOverGround: 123.45f,
        CsUnit: ClassBUnit.Cstdma,
        HasDisplay: true,
        IsAssigned: true,
        IsDscAttached: true,
        Mmsi: 12345,
        Position: new Position(1.0, 2.0),
        PositionAccuracy: true,
        RadioStatusType: ClassBRadioStatusType.Itdma,
        RaimFlag: true,
        RegionalReserved139: 1,
        RegionalReserved38: 2,
        RepeatIndicator: 3,
        SpeedOverGround: 12.34f,
        TimeStampSecond: 56,
        TrueHeadingDegrees: 78);

    private static AisMessageType19 CreateType19() => new(
        CourseOverGround: 123.45f,
        DimensionToBow: 1,
        DimensionToPort: 2,
        DimensionToStarboard: 3,
        DimensionToStern: 4,
        IsAssigned: true,
        IsDteNotReady: true,
        Mmsi: 12345,
        Position: new Position(1.0, 2.0),
        PositionAccuracy: true,
        PositionFixType: EpfdFixType.Gps,
        RaimFlag: true,
        RegionalReserved139: 1,
        RegionalReserved38: 2,
        RepeatIndicator: 3,
        ShipName: "SHIP",
        ShipType: (ShipType)60,
        Spare308: 1,
        SpeedOverGround: 12.34f,
        TimeStampSecond: 56,
        TrueHeadingDegrees: 78);

    private static AisMessageType24Part0 CreateType24Part0() => new(
        Mmsi: 12345,
        PartNumber: 0,
        RepeatIndicator: 3,
        Spare160: 1);

    private static AisMessageType24Part1 CreateType24Part1() => new(
        CallSign: "CALL",
        DimensionToBow: 1,
        DimensionToPort: 2,
        DimensionToStarboard: 3,
        DimensionToStern: 4,
        Mmsi: 12345,
        MothershipMmsi: 54321,
        PartNumber: 1,
        RepeatIndicator: 3,
        SerialNumber: 123,
        Spare162: 1,
        ShipType: (ShipType)60,
        UnitModelCode: 1,
        VendorIdRev3: "VEN",
        VendorIdRev4: "DOR");

    private static AisMessageType27 CreateType27() => new(
        CourseOverGround: 123.45f,
        GnssPositionStatus: true,
        Mmsi: 12345,
        NavigationStatus: NavigationStatus.UnderwayUsingEngine,
        Position: new Position(1.0, 2.0),
        PositionAccuracy: true,
        RaimFlag: true,
        RepeatIndicator: 3,
        SpeedOverGround: 12.34f);
}
