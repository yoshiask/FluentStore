namespace FluentStore.SDK.Models
{
    /// <summary>
    /// Represents a CPU architecture.
    /// </summary>
    /// <remarks>
    /// The lowest 3 bits represent bitness. The upper 4 bits represent the ISA "family".
    /// </remarks>
    public enum Architecture : byte
    {
        Unknown = 0,
        Neutral = 0xF0, // Arm | Intel,

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
        Arm32 = (1 << 4) | bit32,
        Arm64 = (1 << 4) | bit64,

        // Intel / AMD
        Intel = 1 << 5,
        x86 = Intel | bit32,
        x64 = Intel | bit64,
    }

    public static class Architectures
    {
        public static int GetBitnessInt(this Architecture arch)
        {
            int n = (byte)arch & 0xFF;
            // 2^(n + 2)
            return 1 << (n + 2);
        }

        public static Architecture GetBitness(this Architecture arch)
        {
            int n = (byte)arch & 0xFF;
            return (Architecture)n;
        }

        public static Architecture Reduce(this Architecture arch) => arch & Architecture.Neutral;
    }
}
