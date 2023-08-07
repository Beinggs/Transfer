using Beinggs.Transfer.Extensions;
using Beinggs.Transfer.Logging;


namespace Beinggs.Transfer;


// command handlers and helpers
partial class Program
{
	#region Send

	static async Task ToFileCommand (Verbosity? verbosity, string? measured, int port,
			bool repeat, FileInfo? file, bool includeFileName, string recipient)
	{
		try
		{
			if (file is null)
				throw new InvalidOperationException ("A file name must be specified");

			if (string.IsNullOrWhiteSpace (recipient))
				throw new InvalidOperationException ("A recipient must be specified");

			// can't bind globals to root command, so have to handle them in _every_ command :-/
			SetGlobals (verbosity, measured.ToBool(), port);
		
			$"Sending file {file.Name} to {recipient} {SendFileInfo (repeat, includeFileName)}:".Log (LogLevel.Quiet);

			await new Sender (repeat).SendFileAsync (file, includeFileName, recipient);
		}
		catch (Exception ex)
		{
			$"File send failed due to:{Environment.NewLine}{ex.Message}".Log (LogLevel.Error);
		}
	}

	static async Task ToTestCommand (Verbosity? verbosity, string? measured, int port,
			bool repeat, int testSize, string recipient)
	{
		try
		{
			if (testSize is < 1 or > Program.MaxTestSize)
			{
				throw new InvalidOperationException (
						$"Test size must be between {Program.MinTestSize} and {Program.MaxTestSize} (in MiB)");
			}

			if (string.IsNullOrWhiteSpace (recipient))
				throw new InvalidOperationException ("A recipient must be specified");

			// can't bind globals to root command, so have to handle them in _every_ command :-/
			SetGlobals (verbosity, measured.ToBool(), port);

			$"Sending test of {((long) testSize * Size.MiB).HumanSize()} to {recipient} {SendTestInfo (repeat)}"
					.Log (LogLevel.Quiet);

			await new Sender (repeat).SendTestAsync (testSize, recipient);
		}
		catch (Exception ex)
		{
			$"Test send failed due to:{Environment.NewLine}{ex.Message}".Log (LogLevel.Error);
		}
	}

	#endregion Send

	#region Receive

	static async Task FromFileCommand (Verbosity? verbosity, string? measured, int port,
			string? fileName, string sender)
	{
		try
		{
			if (fileName is null)
				throw new InvalidOperationException ("A file name must be specified");

			if (string.IsNullOrWhiteSpace (sender))
				throw new InvalidOperationException ("A sender must be specified");

			// can't bind globals to root command, so have to handle them in _every_ command :-/
			SetGlobals (verbosity, measured.ToBool(), port);

			$"Receiving file from {sender} {ReceiveInfo()}...".Log (LogLevel.Quiet);

			await new Receiver().ReceiveFileAsync (new FileInfo (fileName), sender);
		}
		catch (Exception ex)
		{
			$"File receive failed due to:{Environment.NewLine}{ex.Message}".Log (LogLevel.Error);
		}
	}

	static async Task FromTestCommand (Verbosity? verbosity, string? measured, int port,
			int maxSize, int maxTime, string sender)
	{
		try
		{
			if (maxSize is < 0)
				throw new InvalidOperationException ("A zero or positive maximum size must be specified");

			if (maxTime is < 0)
				throw new InvalidOperationException ("A zero or positive maximum time must be specified");

			if (string.IsNullOrWhiteSpace (sender))
				throw new InvalidOperationException ("A sender must be specified");

			// can't bind globals to root command, so have to handle them in _every_ command :-/
			SetGlobals (verbosity, measured.ToBool(), port);

			($"Receiving {(maxSize > 0 ? $"up to {maxSize} MiB of " : "")}test data " +
				$"{(maxTime > 0 ? $"for up to {maxTime} seconds " : "")}" +
				$"from {sender} {ReceiveInfo()}...").Log (LogLevel.Quiet);

			await new Receiver().ReceiveTestAsync (maxSize, maxTime, sender);
		}
		catch (Exception ex)
		{
			$"Test receive failed due to:{Environment.NewLine}{ex.Message}".Log (LogLevel.Error);
		}
	}

	#endregion Receive

	#region Helpers

	static void SetGlobals (Verbosity? verbosity, bool measured, int port)
	{
		Logger.SetLogLevel (verbosity);

		Measured = measured;
		Port = port;
	}

	static string SendFileInfo (bool repeat, bool includeFileName)
		=> "(" +
			$"log level: {Logger.LogLevel}" +
			$"; {(Measured ? "" : "not ") + "measured"}" +
			$"; port: {Port}" +
			$"; {(repeat ? "" : "not ") + "repeating"}" +
			$"; {(includeFileName ? "" : "not ") + "including file name"}" +
			")";

	static string SendTestInfo (bool repeat)
		=> "(" +
			$"log level: {Logger.LogLevel}" +
			$"; {(Measured ? "" : "not ") + "measured"}" +
			$"; port: {Port}" +
			$"; {(repeat ? "" : "not ") + "repeating"}" +
			")";

	static string ReceiveInfo()
		=> "(" +
			$"log level: {Logger.LogLevel}" +
			$"; {(Measured ? "" : "not ") + "measured"}" +
			$"; port: {Port}" +
			")";

	#endregion Helpers
}
