using System.Buffers;
using System.Diagnostics;


namespace Beinggs.Transfer.Extensions;


/// <summary>
/// Handy-dandy <see cref="Stream"/> extension methods.
/// </summary>
public static class StreamExtensions
{
	/// <summary>
	/// Copies this stream to the <paramref name="destination"/> stream with the given parameters
	/// (based on <see cref="Stream.CopyToAsync (Stream, int, CancellationToken)"/>).
	/// </summary>
	/// <param name="source">The stream to copy from.</param>
	/// <param name="destination">The stream to copy to.</param>
	/// <param name="maxBytes">An optional maximum size, in bytes, to copy (0 is unlimited).</param>
	/// <param name="maxMs">
	/// An optional maximum time, in seconds, to wait for the copy to complete (0 is unlimited).
	/// </param>
	/// <param name="progress">An optional progress reporting callback.</param>
	/// <param name="bufferSize">An optional buffer size, in bytes, to use.</param>
	/// <param name="cancellationToken">An optional cancellation token.</param>
	/// <returns>
	/// A tuple containing the total source read time and total destination write time, in milliseconds, and
	/// the total number of bytes copied.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if an invalid buffer size is given.</exception>
	/// <exception cref="InvalidOperationException">
	/// Thrown of the <paramref name="destination"/> is not writeable.
	/// </exception>
	public static async Task<( long readMs, long writeMs, ulong bytes )> InstrumentedCopyToAsync (this Stream source,
			Stream destination, ulong maxBytes = 0, int maxMs = 0,
			Action<long, long, ulong>? progress = null,
			int bufferSize = 65536, CancellationToken cancellationToken = default)
	{
		if (source is null)
			throw new ArgumentNullException (nameof (source));

		if (destination is null)
			throw new ArgumentNullException (nameof (destination));

		ArgumentNullException.ThrowIfNull (destination);

		if (bufferSize < 1)
			throw new ArgumentOutOfRangeException (nameof (bufferSize), bufferSize,
					"Buffer size must be positive");

		if (!destination.CanWrite)
			throw new InvalidOperationException ("Destination stream is not writeable");

		var totalBytes = 0UL;
		var readTicks = 0L;
		var writeTicks = 0L;

		var buffer = ArrayPool<byte>.Shared.Rent (bufferSize);

		try
		{
			var lastProgress = DateTime.Now;
			var progressInterval = TimeSpan.FromMilliseconds (500);

			var startTime = DateTime.Now;
			var readTimer = Stopwatch.StartNew();
			var writeTimer = Stopwatch.StartNew();

			var bytesRead = 0;

			do
			{
				cancellationToken.ThrowIfCancellationRequested();

				var bytesToRead = maxBytes == 0
					? buffer.Length
					: Math.Min ((int) (maxBytes - (ulong) bytesRead), buffer.Length);

				readTimer.Restart();
				bytesRead = await source.ReadAsync (new Memory<byte> (buffer, 0, bytesToRead), cancellationToken);
				readTicks += readTimer.ElapsedTicks;

				if (bytesRead > 0)
				{
					var foo = totalBytes;
					totalBytes += (ulong) bytesRead;

					writeTimer.Restart();
					await destination.WriteAsync (new ReadOnlyMemory<byte> (buffer, 0, bytesRead), cancellationToken);
					writeTicks += writeTimer.ElapsedTicks;

					if (DateTime.Now > lastProgress + progressInterval)
					{
						lastProgress = DateTime.Now;

						progress?.Invoke (toMilliseconds (readTicks), toMilliseconds (writeTicks), totalBytes);
					}
				}
			} while (bytesRead > 0 &&
					(maxMs == 0 || elapsedMs (startTime) < maxMs) &&
					(maxBytes == 0 || totalBytes < maxBytes));
		}
		finally
		{
			ArrayPool<byte>.Shared.Return (buffer);
		}

		return ( toMilliseconds (readTicks), toMilliseconds (writeTicks), totalBytes );

		static double elapsedMs (DateTime startTime)
			=> (DateTime.Now - startTime).TotalMilliseconds;

		static long toMilliseconds (long ticks)
			=> ticks * 1000 / Stopwatch.Frequency;
	}
}
