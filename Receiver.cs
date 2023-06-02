﻿using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Text;

using Beinggs.Transfer.Extensions;
using Beinggs.Transfer.Logging;
using System;

namespace Beinggs.Transfer;


/// <summary>
/// Receives either a file or some test data from a remote sender.
/// </summary>
public class Receiver
{
	const int _headerSize = 1024;

	const int _kb = 1024;
	const int _mb = _kb * 1024;
	const int _gb = _mb * 1024;

	FileInfo _file = default!;
	ulong _maxBytes;
	int _maxMs;

	/// <summary>
	/// Receives a file from <paramref name="sender"/>.
	/// </summary>
	/// <param name="file">The file to write to.</param>
	/// <param name="sender">The remote sender's IP address or name.</param>
	/// <exception cref="InvalidOperationException">Thrown when an invalid parameter value is given.</exception>
	public Task ReceiveFileAsync (FileInfo file, string sender)
	{
		_file = file;

		return Receive (sender);
	}

	/// <summary>
	/// Receives test data from <paramref name="sender"/>, optionally with size or time limits.
	/// </summary>
	/// <param name="maxSize">An optional maximum size to receive, in MB.</param>
	/// <param name="maxTime">An optional maximum time to receive for, in seconds.</param>
	/// <param name="sender">The sender to receive the test data from.</param>
	/// <exception cref="InvalidOperationException">Thrown when an invalid parameter value is given.</exception>
	public Task ReceiveTestAsync (int maxSize, int maxTime, string sender)
	{
		_maxBytes = (ulong) maxSize * _mb; // max size is MB
		_maxMs = maxTime * 1000; // max time is seconds

		return Receive (sender);
	}

	async Task Receive (string sender)
	{
		try
		{
			var logLevel = Program.Measured ? LogLevel.Quiet : LogLevel.Verbose;

			using TcpClient client = new (sender, Program.Port);

			$"\nConnected to {sender}:{Program.Port}; receiving data...".Log (LogLevel.Quiet);

			using var input = client.GetStream();
			using var output = await GetOutputStream (input);

			// copy the stream to output and time it
			var ( readMs, writeMs, bytes ) = await input.CopyToWithTimingAsync (output, _maxBytes, _maxMs,
					(readMs, writeMs, bytes) => Humanise (readMs, writeMs, bytes).Log (logLevel, "        \r"));

			var receivedMsg = Humanise (readMs, writeMs, bytes, "Total of ");

			if (_file is not null)
				receivedMsg += $" into {_file.Name}";

			receivedMsg.Log (logLevel);

			"Receive complete.".Log (LogLevel.Quiet);
		}
		catch (Exception ex)
		{
			$"Failed to receive data due to: {ex.Message}".Log (LogLevel.Error);
		}
	}

	static string Humanise (long readMs, long writeMs, float bytes, string? prefix = null)
	{
		var bits = bytes * 8f;
		var readSecs = readMs / 1000f;
		var writeSecs = writeMs / 1000f;

		return $"{prefix}{size (bytes)} read in {time (readSecs)} @ {speed (bits / readSecs)}, " +
				$"written in {time (writeSecs)} @ {speed (bits / writeSecs)}";

		static string size (float bytes)
			=> bytes switch
			{
				< _kb => $"{bytes} byte(s)",
				< _mb => $"{bytes / _kb:F3} KB",
				< _gb => $"{bytes / _mb:F3} MB",
				_	  => $"{bytes / _gb:F3} GB"
			};

		static string time (float secs)
			=> secs < 1
				? $"{secs * 1000} ms"
				: $"{secs:F3} sec";

		static string speed (float bps)
			=> bps switch
			{
				< _kb => $"{bps:F2} bps",
				< _mb => $"{bps / _kb:F2} Kbps",
				< _gb => $"{bps / _mb:F2} Mbps",
				_	  => $"{bps / _gb:F2} Gbps"
			};
	}

	async Task<Stream> GetOutputStream (Stream input)
	{
		if (_file is null)
			return new DiscardStream();

		// to avoid DoS attack, only check first K of data for headers
		var headerBytes = new byte [_headerSize];
		var headerLen = await input.ReadAsync (headerBytes);

		using var header = new MemoryStream (headerBytes, 0, headerLen, writable: false);

		if (GetFileName (header, $"{Program.FileNameHeader}:",
				out var fileName, out var lineRead))
		{
			// if no filename given on cmd line, use the one from the header
			if (_file.Name == Program.DefaultReceiveFileName)
			{
#if DEBUG
				fileName = "TEST." + fileName;
#endif
				// we got a filename, so create that file instead of the default
				_file = new FileInfo (fileName);
			}
		}

		var bytesConsumed = lineRead is null ? 0 : lineRead.ToUtf8Bytes().Length +
				fileName is null ? 0 : 1; // if we got a filename, allow for the extra '\n'

		var output = _file.OpenWrite();

		await output.WriteAsync (new ReadOnlyMemory<byte> (headerBytes, bytesConsumed, headerLen - bytesConsumed));

		return output;
	}

	static bool GetFileName (Stream input, string prefix,
			[NotNullWhen (true)] out string? fileName, out string? lineRead)
	{
		using StreamReader reader = new (input, Encoding.UTF8, leaveOpen: true);

		lineRead = reader.ReadLine();
		fileName = null;

		if (lineRead?.StartsWith (prefix) == true)
		{
			fileName = lineRead [prefix.Length..].Trim();

			return !string.IsNullOrEmpty (fileName);
		}

		return false;
	}
}
