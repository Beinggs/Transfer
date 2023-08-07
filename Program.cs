using System.CommandLine;


namespace Beinggs.Transfer;


/// <summary>
/// Implements a simple test data and file transfer command-line utility.
/// </summary>
/// <remarks>
/// See the README.md file for details of operation.
/// </remarks>
public partial class Program
{
	/// <summary>
	/// Defines the entry point for <see cref="Program"/>.
	/// </summary>
	/// <param name="args"></param>
	/// <returns></returns>
	static async Task<int> Main (string[] args)
	{
		// as commands can't be declared with aliases, we have to declare them here and add aliases later <grr!>
		Command sendCommand;
		Command sendFileCommand;
		Command sendTestCommand;

		Command receiveCommand;
		Command receiveFileCommand;
		Command receiveTestCommand;

		// build the command line...
		var arg0 = Environment.GetCommandLineArgs() [0];
		var exeName = Path.GetFileNameWithoutExtension (arg0);

		RootCommand rootCommand = new ($"Beinggs Transfer ({exeName}) - Simple Transfer App")
		{
			(sendCommand = new (
				name: "send",
				description: "Send a file or test data")
			{
				(sendFileCommand = new (
					name: "file",
					description: "Send a file")
				{
					argFile,
					optIncludeFileName,
					cmdFileTo
				}),
				(sendTestCommand = new (
					name: "test",
					description: "Send test data")
				{
					cmdTestTo
				}),
				cmdTestTo // implicit 'test' command
			}),

			(receiveCommand = new (
				name: "receive",
				description: "Receive a file or test data")
			{
				(receiveFileCommand = new (
					name: "file",
					description: "Receive a file")
				{
					argFileName, // this is a string?, so can't use argFile as that's a FileInfo?
					cmdFileFrom
				}),
				(receiveTestCommand = new (
					name: "test",
					description: "Receive test data")
				{
					cmdTestFrom
				}),
				cmdTestFrom // implicit 'test' command
			})
		};

		// ... add aliases...
		sendCommand.AddAlias ("s");
		sendFileCommand.AddAlias ("f");
		sendTestCommand.AddAlias ("t");

		receiveCommand.AddAlias ("r");
		receiveFileCommand.AddAlias ("f");
		receiveTestCommand.AddAlias ("t");

		// ... add globals...
		globalOptVerbosity.Arity = ArgumentArity.ZeroOrOne;	// verbosity with no value defaults to verbose
		globalOptMeasured.Arity = ArgumentArity.ZeroOrOne;	// measured with no value defaults to measured

		rootCommand.AddGlobalOption (globalOptVerbosity);
		rootCommand.AddGlobalOption (globalOptMeasured);
		rootCommand.AddGlobalOption (globalOptPort);

		// ... set handlers...
		// rootCommand.SetHandler (SetLogLevel, verbosity); // GRR! This should be supported, at least for globals! :-/
		cmdFileTo.SetHandler (ToFileCommand, globalOptVerbosity, globalOptMeasured, globalOptPort, optRepeat,
				argFile, optIncludeFileName, argRecipient);

		cmdTestTo.SetHandler (ToTestCommand, globalOptVerbosity, globalOptMeasured, globalOptPort, optRepeat,
				optTestSize, argRecipient);

		cmdFileFrom.SetHandler (FromFileCommand, globalOptVerbosity, globalOptMeasured, globalOptPort,
				argFileName, argSender);

		cmdTestFrom.SetHandler (FromTestCommand, globalOptVerbosity, globalOptMeasured, globalOptPort,
				optMaxSize, optMaxTime, argSender);

		// ... and let the magic happen!
		return await rootCommand.InvokeAsync (args);
	}
}
