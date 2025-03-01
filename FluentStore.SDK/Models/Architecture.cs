namespace FluentStore.SDK.Models
{
    /// <summary>
    /// Represents a CPU architecture.
    /// </summary>
    /// <remarks>
    /// The lowest 3 bits represent bitness.
    /// Bits 4 through 7 represent the target ISA family.
    /// Bits 8 through 15 represent the emulated ISA family.
    /// </remarks>
    public enum Architecture : uint
    {
        Unknown = 0,
        Neutral = 0x00F0, // Arm | Intel,

        // "Bitness"
        bit8 = 1,
        bit16 = 2,
        bit32 = 3,
        bit64 = 4,
        bit128 = 5,
        bit256 = 6,
        bit512 = 7,

        // ARM
        Arm = 1 << 4,
        Arm32 = Arm | bit32,
        Arm64 = Arm | bit64,

        // Intel / AMD
        Intel = 1 << 5,
        x86 = Intel | bit32,
        x64 = Intel | bit64,

        // x86 on ARM64
        x86a64 = (x86 << 8) | Arm64
    }

    public static class Architectures
    {
        public static int GetBitnessInt(this Architecture arch)
        {
            var n = (byte)(arch & Architecture.bit512);
            // 2^(n + 2)
            return 1 << (n + 2);
        }

        public static Architecture GetBitness(this Architecture arch)
        {
            var n = (uint)arch & 0xFFu;
            return (Architecture)n;
        }

        public static Architecture Reduce(this Architecture arch) => arch & Architecture.Neutral;

        public static Architecture Target(this Architecture arch) => (Architecture)((uint)arch & 0xFFu);

        public static Architecture Emulated(this Architecture arch) => (Architecture)((uint)arch >> 8);
    }
}
