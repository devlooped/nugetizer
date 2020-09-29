using System;
using System.Linq;
using System.Numerics;
using System.Text;

/// <summary>
/// Inspired in a bunch of searches, samples and snippets on various languages 
/// and blogs and what-not on doing URL shortering :), heavily tweaked to make 
/// it fully idiomatic in C#.
/// </summary>
public static class Base62
{
    /// <summary>
    /// Encodes a numeric value into a base62 string.
    /// </summary>
    public static string Encode(BigInteger value)
    {
        // TODO: I'm almost sure there's a more succint way 
        // of doing this with LINQ and Aggregate, but just 
        // can't figure it out...
        var sb = new StringBuilder();

        while (value != 0)
        {
            sb = sb.Append(ToBase62(value % 62));
            value /= 62;
        }

        return new string(sb.ToString().Reverse().ToArray());
    }

    /// <summary>
    /// Decodes a base62 string into its original numeric value.
    /// </summary>
    public static BigInteger Decode(string value)
        => value.Aggregate(new BigInteger(0), (current, c) => current * 62 + FromBase62(c));

    static char ToBase62(BigInteger d) => d switch
    {
        BigInteger v when v < 10 => (char)('0' + d),
        BigInteger v when v < 36 => (char)('A' + d - 10),
        BigInteger v when v < 62 => (char)('a' + d - 36),
        _ => throw new ArgumentException($"Cannot encode digit {d} to base 62.", nameof(d)),
    };

    static BigInteger FromBase62(char c) => c switch
    {
        char v when c >= 'a' && v <= 'z' => 36 + c - 'a',
        char v when c >= 'A' && v <= 'Z' => 10 + c - 'A',
        char v when c >= '0' && v <= '9' => c - '0',
        _ => throw new ArgumentException($"Cannot decode char '{c}' from base 62.", nameof(c)),
    };
}
