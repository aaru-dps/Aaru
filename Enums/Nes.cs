namespace Aaru.CommonTypes.Enums;

public enum NesConsoleType : byte
{
    Nes        = 0,
    Vs         = 1,
    Playchoice = 2,
    Extended   = 3
}

public enum NesTimingMode : byte
{
    RP2C02   = 0,
    RP2C07   = 1,
    Multiple = 2,
    UMC6527P = 3
}

public enum NesVsPpuType : byte
{
    RP2C03B     = 0,
    RP2C03G     = 1,
    RP2C04_0001 = 2,
    RP2C04_0002 = 3,
    RP2C04_0003 = 4,
    RP2C04_0004 = 5,
    RC2C03B     = 6,
    RC2C03C     = 7,
    RC2C05_01   = 8,
    RC2C05_02   = 9,
    RC2C05_03   = 10,
    RC2C05_04   = 11,
    RC2C05_05   = 12
}

public enum NesVsHardwareType : byte
{
    Normal          = 0,
    RBI             = 1,
    TKO             = 2,
    SuperXevious    = 3,
    IceClimber      = 4,
    Dual            = 5,
    RaidOnBungeling = 6
}

public enum NesExtendedConsoleType : byte
{
    Normal          = 0,
    Vs              = 1,
    Playchoice      = 2,
    DecimalMode     = 3,
    VT01_Monochrome = 4,
    VT01            = 5,
    VT02            = 6,
    VT03            = 7,
    VT09            = 8,
    VT32            = 9,
    VT369           = 10,
    UM6578          = 11
}

public enum NesDefaultExpansionDevice : byte
{
    Unspecified                     = 0,
    Controller                      = 1,
    FourScore                       = 2,
    FourPlayersAdapter              = 3,
    Vs                              = 4,
    VsSystem                        = 5,
    VsPinball                       = 6,
    VsZapper                        = 7,
    Zapper                          = 8,
    TwoZappers                      = 9,
    HyperShotLightgun               = 0xA,
    PowerPadSideA                   = 0xB,
    PowerPadSideB                   = 0xC,
    FamilyTrainerSideA              = 0xD,
    FamilyTrainerSideB              = 0xE,
    ArkanoidVaus                    = 0xF,
    ArkanoidVausFamicom             = 0x10,
    TwoVausDataRecorder             = 0x11,
    HyperShotController             = 0x12,
    CoconutsPachinko                = 0x13,
    ExcitingBoxing                  = 0x14,
    JissenMahjong                   = 0x15,
    PartyTap                        = 0x16,
    OekaKidsTablet                  = 0x17,
    SunsoftBarcodeBattler           = 0x18,
    PianoKeyboard                   = 0x19,
    PokkunMoguraa                   = 0x1A,
    TopRider                        = 0x1B,
    DoubleFisted                    = 0x1C,
    Famicom3DSystem                 = 0x1D,
    DoremikkoKeyboard               = 0x1E,
    GyroSet                         = 0x1F,
    DataRecorder                    = 0x20,
    TurboFile                       = 0x21,
    StorageBattleBox                = 0x22,
    FamilyBASICKeyboardDataRecorder = 0x23,
    DongdaKeyboard                  = 0x24,
    BitCorpKeyboard                 = 0x25,
    SuborKeyboard                   = 0x26,
    SuborKeyboardMouse              = 0x27,
    SuborKeyboardMouse24            = 0x28,
    SNESMouse                       = 0x29,
    Multicart                       = 0x2A,
    SNESControllers                 = 0x2B,
    RacerMateBicycle                = 0x2C,
    UForce                          = 0x2D,
    StackUp                         = 0x2E,
    PatrolmanLightgun               = 0x2F,
    C1CassetteInterface             = 0x30,
    SwappedController               = 0x31,
    SudokuPad                       = 0x32,
    ABLPinball                      = 0x33,
    GoldenNuggetCasino              = 0x34
}