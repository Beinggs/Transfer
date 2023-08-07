namespace Beinggs.Transfer.Extensions;


/// <summary>
/// Handy-dandy extension methods for numeric values.
/// </summary>
public static class NumberExtensions
{
	/// <summary>
	/// Formats the given byte count to a human-readable string.
	/// </summary>
	/// <param name="bytes"></param>
	public static string HumanSize (this int bytes)
		=> HumanSize ((float) bytes);

	/// <summary>
	/// Formats the given byte count to a human-readable string.
	/// </summary>
	/// <param name="bytes"></param>
	public static string HumanSize (this long bytes)
		=> HumanSize ((float) bytes);

	/// <summary>
	/// Formats the given byte count to a human-readable string.
	/// </summary>
	/// <param name="bytes"></param>
	public static string HumanSize (this float bytes)
		=> bytes switch
		{
			< Size.KiB => $"{bytes} byte(s)",
			< Size.MiB => $"{bytes / Size.KiB:F3} KiB",
			< Size.GiB => $"{bytes / Size.MiB:F3} MiB",
			_          => $"{bytes / Size.GiB:F3} GiB"
		};

	/// <summary>
	/// Formats the given bits-per-second value to a human-readable string.
	/// </summary>
	/// <param name="bps"></param>
	public static string HumanSpeed (this int bps)
		=> HumanSpeed ((float) bps);

	/// <summary>
	/// Formats the given bits-per-second value to a human-readable string.
	/// </summary>
	/// <param name="bps"></param>
	public static string HumanSpeed (this long bps)
		=> HumanSpeed ((float) bps);

	/// <summary>
	/// Formats the given bits-per-second value to a human-readable string.
	/// </summary>
	/// <param name="bps"></param>
	public static string HumanSpeed (this float bps)
		=> bps switch
		{
			< Size.KiB => $"{bps:F2} bit/s",
			< Size.MiB => $"{bps / Size.KiB:F2} Kibit/s",
			< Size.GiB => $"{bps / Size.MiB:F2} Mibit/s",
			_          => $"{bps / Size.GiB:F2} Gibit/s"
		};

	/// <summary>
	/// Formats the given seconds value to a human-readable string.
	/// </summary>
	/// <param name="secs"></param>
	public static string HumanTime (this int secs)
		=> HumanTime ((float) secs);

	/// <summary>
	/// Formats the given seconds value to a human-readable string.
	/// </summary>
	/// <param name="secs"></param>
	public static string HumanTime (this long secs)
		=> HumanTime ((float) secs);

	/// <summary>
	/// Formats the given seconds value to a human-readable string.
	/// </summary>
	/// <param name="secs"></param>
	public static string HumanTime (this float secs)
		=> secs < 1
			? $"{secs * 1000} ms"
			: $"{secs:F3} sec";
}

/// <summary>
/// Defines some handy size constants.
/// </summary>
public static class Size
{
	/// <summary>
	/// One kibibyte.
	/// </summary>
	public const int KiB = 1024;

	/// <summary>
	/// One mebibyte.
	/// </summary>
	public const int MiB = KiB * KiB;

	/// <summary>
	/// One gibibyte.
	/// </summary>
	public const int GiB = MiB * KiB;
}
