using System.CommandLine;

using Beinggs.Transfer.Logging;


namespace Beinggs.Transfer;


// command line definitions: Commands, Arguments and Options
partial class Program
{
	#region Fields

	/// <summary>
	/// Defines the default received file name (if the <c>includeFileName</c> option isn't specified).
	/// </summary>
	public const string DefaultReceiveFileName = "transfer.dat";

	/// <summary>
	/// Defines the header key for the filename header, if provided.
	/// </summary>
	public const string FileNameHeader = "filename";

	/// <summary>
	/// Defines the 'anyone' string to specify sending/receiving to/from anyone.
	/// </summary>
	public static readonly string[] Anyone = { "anyone", "any", "a" };

	/// <summary>
	/// Defines the minimum test size option value, in MiB.
	/// </summary>
	public const int MinTestSize = 1; // MiB

	/// <summary>
	/// Defines the maximum test size option value, in MiB.
	/// </summary>
	public const int MaxTestSize = 10240; // MiB

	/// <summary>
	/// Defines the default test size option value, in MiB.
	/// </summary>
	public const int DefTestSize = 10; // MiB

	/// <summary>
	/// Indicates whether performance should be measured when receiving test data or a file.
	/// </summary>
	public static bool Measured { get; set; }

	/// <summary>
	/// Provides the port value to connect to or listen on.
	/// </summary>
	public static int Port { get; set; }

	#endregion Fields

	#region Global options

	static readonly Option<Verbosity?> globalOptVerbosity = new (
		aliases: new [] { "--verbosity", "-v", "/v" },
		description: "Level of detail in output messages",
		getDefaultValue: () => Verbosity.Normal);

	static readonly Option<string?> globalOptMeasured = new (
		aliases: new[] { "--measured", "-m", "/m" },
		description: "Set false to hide timing and perf data",
		getDefaultValue: () => "true");

	static readonly Option<int> globalOptPort = new (
		aliases: new [] { "--port", "-p", "/p" },
		description: "Specifies the port to listen or send on",
		getDefaultValue: () => 8888);

	#endregion Global options

	#region Send stuff

	static readonly Option<bool> optRepeat = new (
		aliases: new[] { "--repeat", "-r", "/r" },
		description: "Set true to repeatedly send",
		getDefaultValue: () => false);

	static readonly Argument<FileInfo?> argFile = new (
		name: "fileName",
		description: "The name of the file to send");

	static readonly Option<bool> optIncludeFileName = new (
		aliases: new[] { "--include-filename", "-f", "/f" },
		description: $"Set false to send raw data with no \"{FileNameHeader}:name\\n\" (as UTF8 bytes) header",
		getDefaultValue: () => true);

	static readonly Argument<string> argRecipient = new (
		name: "recipient",
		description: $"Recipient machine, IP address, or {GetAnyoneList()} to allow any remote client to connect");

	static readonly Option<int> optTestSize = new (
		aliases: new[] { "--size", "-s", "/s" },
		description: $"Test data size, in MiB, between {MinTestSize} and {MaxTestSize}",
		getDefaultValue: () => DefTestSize);

	static readonly Command cmdFileTo = new (
		name: "to",
		description: "Specifies the recipient of the file data")
	{
		argRecipient,
		optRepeat
	};

	static readonly Command cmdTestTo = new (
		name: "to",
		description: "Specifies the recipient of the test data")
	{
		argRecipient,
		optRepeat,
		optTestSize
	};

	static string GetAnyoneList()
	{
		var s = $"'{Anyone [0]}' (";
		for (var i = 0; ++i < Anyone.Length; )
			s += $"or '{Anyone [i]}'" +
					(i < Anyone.Length - 1 ? ", " : ")");

		return s;
	}

	#endregion Send stuff

	#region Receive stuff

	static readonly Argument<string?> argFileName = new ( // nullable type makes it optional, so default will kick in
		name: "fileName",
		description: "The name of the file to receive",
		getDefaultValue: () => DefaultReceiveFileName);

	static readonly Option<int> optMaxSize = new (
		aliases: new[] { "--max-size", "-s", "/s" },
		description: "Sets the maximum amount of test data to receive, in MiB, " +
				"or omit (or set to zero) to receive all test data sent",
		getDefaultValue: () => 0);

	static readonly Option<int> optMaxTime = new (
		aliases: new[] { "--max-time", "-t", "/t" },
		description: "Sets the maximum amount of time to receive test data for, in seconds, " +
				"or omit (or set to zero) to receive all test data sent",
		getDefaultValue: () => 0);

	static readonly Argument<string> argSender = new (
		name: "sender",
		description: "Sender machine or IP address");

	static readonly Command cmdFileFrom = new (
		name: "from",
		description: "Specifies the sender of the file data")
	{
		argSender
	};

	static readonly Command cmdTestFrom = new (
		name: "from",
		description: "Specifies the sender of the test data")
	{
		argSender,
		optMaxSize,
		optMaxTime
	};

	#endregion Receive stuff
}
