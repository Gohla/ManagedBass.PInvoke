﻿using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ManagedBass.Enc
{
    /// <summary>
    /// Wraps BassEnc: bassenc.dll
    /// </summary>
    public static partial class BassEnc
    {
        static IntPtr _castProxy;
        
        #region Version
        [DllImport(DllName)]
        static extern int BASS_Encode_GetVersion();

        /// <summary>
        /// Gets the Version of BassEnc that is loaded.
        /// </summary>
        public static Version Version => Extensions.GetVersion(BASS_Encode_GetVersion());
        #endregion

        #region Configure
        /// <summary>
        /// Encoder DSP priority (default -1000) which determines where in the DSP chain the encoding is performed. 
        /// </summary>
        /// <remarks>
        /// All DSP with a higher priority will be present in the encoding.
        /// Changes only affect subsequent encodings, not those that have already been started.
        /// </remarks>
        public static int DSPPriority
        {
            get { return Bass.GetConfig(Configuration.EncodePriority); }
            set { Bass.Configure(Configuration.EncodePriority, value); }
        }

        /// <summary>
        /// The maximum queue Length of the async encoder (default 10000, 0 = Unlimited) in milliseconds.
        /// </summary>
        /// <remarks>
        /// When queued encoding is enabled, the queue's Buffer will grow as needed to hold the queued data, up to a limit specified by this config option.
        /// Changes only apply to new encoders, not any already existing encoders.
        /// </remarks>
        public static int Queue
        {
            get { return Bass.GetConfig(Configuration.EncodeQueue); }
            set { Bass.Configure(Configuration.EncodeQueue, value); }
        }

        /// <summary>
        /// The time to wait (in milliseconds) to send data to a cast server (default 5000ms)
        /// </summary>
        /// <remarks>
        /// When an attempt to send data is timed-out, the data is discarded. 
        /// <see cref="EncodeSetNotify"/> can be used to receive a notification of when this happens.
        /// Changes take immediate effect.
        /// </remarks>
        public static int CastTimeout
        {
            get { return Bass.GetConfig(Configuration.EncodeCastTimeout); }
            set { Bass.Configure(Configuration.EncodeCastTimeout, value); }
        }

        /// <summary>
        /// Proxy server settings when connecting to Icecast and Shoutcast (in the form of "[User:pass@]server:port"... <see langword="null"/> (default) = don't use a proxy but a direct connection).
        /// </summary>
        /// <remarks>
        /// If only the "server:port" part is specified, then that proxy server is used without any authorization credentials.
        /// This setting affects how the following functions connect to servers: <see cref="CastInit"/>, <see cref="CastGetStats"/>, <see cref="CastSetTitle(int, string, string)"/>.
        /// When a proxy server is used, it needs to support the HTTP 'CONNECT' method.
        /// The default setting is <see langword="null"/> (do not use a proxy).
        /// Changes take effect from the next internet stream creation call. 
        /// By default, BassEnc will not use any proxy settings when connecting to Icecast and Shoutcast.
        /// </remarks>
        public static string CastProxy
        {
            // BassEnc does not make a copy of the config string, so it must reside in the heap (not the stack), eg. a global variable. 
            // This also means that the proxy settings can subsequently be changed at that location without having to call this function again.

            get { return Marshal.PtrToStringAnsi(Bass.GetConfigPtr(Configuration.EncodeCastProxy)); }
            set
            {
                if (_castProxy != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_castProxy);

                    _castProxy = IntPtr.Zero;
                }

                _castProxy = Marshal.StringToHGlobalAnsi(value);

                Bass.Configure(Configuration.EncodeCastProxy, _castProxy);
            }
        }
        #endregion

        #region Encoding
        /// <summary>
        /// Sends a RIFF chunk to an encoder.
        /// </summary>
        /// <param name="Handle">The encoder handle... a HENCODE.</param>
        /// <param name="ID">The 4 character chunk id (e.g. 'bext').</param>
        /// <param name="Buffer">The buffer containing the chunk data (without the id).</param>
        /// <param name="Length">The number of bytes in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// BassEnc writes the minimum chunks required of a WAV file: "fmt" and "data", and "ds64" and "fact" when appropriate.
        /// This function can be used to add other chunks. 
        /// For example, a BWF "bext" chunk or "INFO" tags.
        /// <para>
        /// Chunks can only be added prior to sample data being sent to the encoder.
        /// The <see cref="EncodeFlags.Pause"/> flag can be used when starting the encoder to ensure that no sample data is sent before additional chunks have been set.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">No RIFF headers/chunks are being sent to the encoder (due to the <see cref="EncodeFlags.NoHeader"/> flag being in effect), or sample data encoding has started.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_AddChunk")]
        public static extern bool EncodeAddChunk(int Handle, string ID, IntPtr Buffer, int Length);

        /// <summary>
        /// Sends a RIFF chunk to an encoder.
        /// </summary>
        /// <param name="Handle">The encoder handle... a HENCODE.</param>
        /// <param name="ID">The 4 character chunk id (e.g. 'bext').</param>
        /// <param name="Buffer">The buffer containing the chunk data (without the id).</param>
        /// <param name="Length">The number of bytes in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// BassEnc writes the minimum chunks required of a WAV file: "fmt" and "data", and "ds64" and "fact" when appropriate.
        /// This function can be used to add other chunks. 
        /// For example, a BWF "bext" chunk or "INFO" tags.
        /// <para>
        /// Chunks can only be added prior to sample data being sent to the encoder.
        /// The <see cref="EncodeFlags.Pause"/> flag can be used when starting the encoder to ensure that no sample data is sent before additional chunks have been set.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">No RIFF headers/chunks are being sent to the encoder (due to the <see cref="EncodeFlags.NoHeader"/> flag being in effect), or sample data encoding has started.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_AddChunk")]
        public static extern bool EncodeAddChunk(int Handle, string ID, byte[] Buffer, int Length);
        
        /// <summary>
        /// Retrieves the channel that an encoder is set on.
        /// </summary>
        /// <param name="Handle">The encoder to get the channel from.</param>
        /// <returns>If successful, the encoder's channel handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_GetChannel")]
        public static extern int EncodeGetChannel(int Handle);

        /// <summary>
        /// Retrieves the amount of data queued, sent to or received from an encoder, or sent to a cast server.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <param name="Count">The count to retrieve (see <see cref="EncodeCount"/>).</param>
        /// <returns>If successful, the requested count (in bytes) is returned, else -1 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// The queue counts are based on the channel's sample format (floating-point if the <see cref="Bass.FloatingPointDSP"/> option is enabled),
        /// while the <see cref="EncodeCount.In"/> count is based on the sample format used by the encoder,
        /// which could be different if one of the Floating-point conversion flags is active or the encoder is using an ACM codec (which take 16-bit data).
        /// </para>
        /// <para>
        /// When the encoder output is being sent to a cast server, the <see cref="EncodeCount.Cast"/> count will match the <see cref="EncodeCount.Out"/> count,
        /// unless there have been problems (eg. network timeout) that have caused data to be dropped.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">The encoder does not have a queue.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Count" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_GetCount")]
        public static extern long EncodeGetCount(int Handle, EncodeCount Count);

		/// <summary>
		/// Checks if an encoder is running on a channel.
		/// </summary>
		/// <param name="Handle">The encoder or channel handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
		/// <returns>The return value is one of <see cref="PlaybackState"/> values.</returns>
		/// <remarks>
		/// <para>When checking if there's an encoder running on a channel, and there are multiple encoders on the channel, <see cref="PlaybackState.Playing"/> will be returned if any of them are active.</para>
		/// <para>If an encoder stops running prematurely, <see cref="EncodeStop(int)" /> should still be called to release resources that were allocated for the encoding.</para>
		/// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_Encode_IsActive")]
        public static extern PlaybackState EncodeIsActive(int Handle);
        
		/// <summary>
		/// Moves an encoder (or all encoders on a channel) to another channel.
		/// </summary>
		/// <param name="Handle">The encoder or channel handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
		/// <param name="Channel">The channel to move the encoder(s) to... a HSTREAM, HMUSIC, or HRECORD.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
        /// The new channel must have the same sample format (rate, channels, resolution) as the old channel, as that is what the encoder is expecting. 
		/// A channel's sample format is available via <see cref="Bass.ChannelGetInfo(int, out ChannelInfo)" />.
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> or <paramref name="Channel" /> is not valid.</exception>
        /// <exception cref="Errors.SampleFormat">The new channel's sample format is not the same as the old channel's.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_SetChannel")]
        public static extern bool EncodeSetChannel(int Handle, int Channel);

        /// <summary>
        /// Sets a callback function on an encoder (or all encoders on a channel) to receive notifications about its status.
        /// </summary>
        /// <param name="Handle">The encoder or channel handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Procedure">Callback function to receive the notifications... <see langword="null" /> = no callback.</param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// When setting a notification callback on a channel, it only applies to the encoders that are currently set on the channel.
        /// Subsequent encoders will not automatically have the notification callback set on them, this function will have to be called again to set them up.
        /// </para>
        /// <para>
        /// An encoder can only have one notification callback set.
        /// Subsequent calls of this function can be used to change the callback function, or disable notifications (<paramref name="Procedure"/> = <see langword="null" />).
        /// </para>
        /// <para>
        /// The status of an encoder and its cast connection (if it has one) is checked when data is sent to the encoder or server, and by <see cref="EncodeIsActive" />.
        /// That means an encoder's death will not be detected automatically, and so no notification given, while no data is being encoded.
        /// </para>
        /// <para>If the encoder is already dead when setting up a notification callback, the callback will be triggered immediately.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_SetNotify")]
        public static extern bool EncodeSetNotify(int Handle, EncodeNotifyProcedure Procedure, IntPtr User = default(IntPtr));
        
		/// <summary>
		/// Pauses or resumes encoding on a channel.
		/// </summary>
		/// <param name="Handle">The encoder or channel handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
		/// <param name="Paused">Paused?</param>
		/// <returns>If no encoder has been started on the channel, <see langword="false" /> is returned, otherwise <see langword="true" /> is returned.</returns>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
        /// When an encoder is paused, no sample data will be sent to the encoder "automatically".
        /// Data can still be sent to the encoder "manually" though, via the <see cref="EncodeWrite(int, IntPtr, int)" /> function.
        /// </para>
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.s</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_SetPaused")]
        public static extern bool EncodeSetPaused(int Handle, bool Paused = true);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_Start(int handle, string cmdline, EncodeFlags flags, EncodeProcedure proc, IntPtr user);

        public static int EncodeStart(int handle, string cmdline, EncodeFlags flags, EncodeProcedure proc, IntPtr user = default(IntPtr))
        {
            return BASS_Encode_Start(handle, cmdline, flags | EncodeFlags.Unicode, proc, user);
        }

#if __MAC__ || __IOS__
        [DllImport(DllName, EntryPoint = "BASS_Encode_StartCA")]
        public static extern int EncodeStartCA(int handle, int ftype, int atype, EncodeFlags flags, int bitrate, EncodeProcedureEx proc, IntPtr user);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_StartCAFile(int handle, int ftype, int atype, EncodeFlags flags, int bitrate, string filename);

        public static int EncodeStartCA(int handle, int ftype, int atype, EncodeFlags flags, int bitrate, string filename)
        {
            return BASS_Encode_StartCAFile(handle, ftype, atype, flags | EncodeFlags.Unicode, bitrate, filename);
        }
#endif

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_StartLimit(int handle, string cmdline, EncodeFlags flags, EncodeProcedure proc, IntPtr user, int limit);

        public static int EncodeStart(int handle, string cmdline, EncodeFlags flags, EncodeProcedure proc, IntPtr user, int limit)
        {
            return BASS_Encode_StartLimit(handle, cmdline, flags | EncodeFlags.Unicode, proc, user, limit);
        }

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_StartUser(int handle, string filename, EncodeFlags flags, EncoderProcedure proc, IntPtr user);

        public static int EncodeStart(int handle, string filename, EncodeFlags flags, EncoderProcedure proc, IntPtr user = default(IntPtr))
        {
            return BASS_Encode_StartUser(handle, filename, flags | EncodeFlags.Unicode, proc, user);
        }
        
		/// <summary>
		/// Stops encoding on a channel.
		/// </summary>
		/// <param name="Handle">The encoder or channel handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// This function will free an encoder immediately, without waiting for any data that may be remaining in the queue.
        /// <see cref="EncodeStop(int, bool)" /> can be used to have an encoder process the queue before it is freed.
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_Stop")]
        public static extern bool EncodeStop(int Handle);
        
		/// <summary>
		/// Stops async encoding on a channel.
		/// </summary>
		/// <param name="Handle">The encoder or channel handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
		/// <param name="Queue">Process the queue first? If so, the encoder will not be freed until after any data remaining in the queue has been processed, and it will not accept any new data in the meantime.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// When an encoder is told to wait for its queue to be processed, this function will return immediately and the encoder will be freed in the background after the queued data has been processed.
		/// <see cref="EncodeSetNotify" /> can be used to request notification of when the encoder has been freed.
        /// <see cref="EncodeStop(int)" /> (or this function with queue = <see langword="false" />) can be used to cancel to queue processing and free the encoder immediately.
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_StopEx")]
        public static extern bool EncodeStop(int Handle, bool Queue);

        #region Encode Write
        /// <summary>
        /// Sends sample data to the encoder.
        /// </summary>
        /// <param name="Handle">The encoder or channel handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Buffer">A pointer to the buffer containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// There's usually no need to use this function, as the channel's sample data will automatically be fed to the encoder.
        /// But in some situations, it could be useful to be able to manually feed the encoder instead.
        /// <para>The sample data is expected to be the same format as the channel's, or floating-point if the <see cref="Bass.FloatingPointDSP"/> option is enabled.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_Write")]
        public static extern bool EncodeWrite(int Handle, IntPtr Buffer, int Length);

        /// <summary>
        /// Sends sample data to the encoder.
        /// </summary>
        /// <param name="Handle">The encoder or channel handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Buffer">byte[] containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// There's usually no need to use this function, as the channel's sample data will automatically be fed to the encoder.
        /// But in some situations, it could be useful to be able to manually feed the encoder instead.
        /// <para>The sample data is expected to be the same format as the channel's, or floating-point if the <see cref="Bass.FloatingPointDSP"/> option is enabled.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_Write")]
        public static extern bool EncodeWrite(int Handle, byte[] Buffer, int Length);

        /// <summary>
        /// Sends sample data to the encoder.
        /// </summary>
        /// <param name="Handle">The encoder or channel handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Buffer">short[] containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// There's usually no need to use this function, as the channel's sample data will automatically be fed to the encoder.
        /// But in some situations, it could be useful to be able to manually feed the encoder instead.
        /// <para>The sample data is expected to be the same format as the channel's, or floating-point if the <see cref="Bass.FloatingPointDSP"/> option is enabled.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_Write")]
        public static extern bool EncodeWrite(int Handle, short[] Buffer, int Length);

        /// <summary>
        /// Sends sample data to the encoder.
        /// </summary>
        /// <param name="Handle">The encoder or channel handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Buffer">int[] containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// There's usually no need to use this function, as the channel's sample data will automatically be fed to the encoder.
        /// But in some situations, it could be useful to be able to manually feed the encoder instead.
        /// <para>The sample data is expected to be the same format as the channel's, or floating-point if the <see cref="Bass.FloatingPointDSP"/> option is enabled.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_Write")]
        public static extern bool EncodeWrite(int Handle, int[] Buffer, int Length);

        /// <summary>
        /// Sends sample data to the encoder.
        /// </summary>
        /// <param name="Handle">The encoder or channel handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Buffer">float[] containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// There's usually no need to use this function, as the channel's sample data will automatically be fed to the encoder.
        /// But in some situations, it could be useful to be able to manually feed the encoder instead.
        /// <para>The sample data is expected to be the same format as the channel's, or floating-point if the <see cref="Bass.FloatingPointDSP"/> option is enabled.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_Write")]
        public static extern bool EncodeWrite(int Handle, float[] Buffer, int Length);
        #endregion
        #endregion

        #region Casting
        [DllImport(DllName)]
        static extern IntPtr BASS_Encode_CastGetStats(int handle, EncodeStats type, [In] string pass);

        /// <summary>
        /// Retrieves stats from the Shoutcast or Icecast server.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <param name="Type">The type of stats to retrieve.</param>
        /// <param name="Password">Password when retrieving Icecast server stats... <see langword="null" /> = use the password provided in the <see cref="CastInit" /> call.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The stats are returned in XML format.
        /// <para>
        /// Each encoder has a single stats buffer, which is reused by each call of this function for the encoder. 
        /// So if the data needs to be retained across multiple calls, it should be copied to another buffer.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Type"><paramref name="Type" /> is invalid.</exception>
        /// <exception cref="Errors.NotAvailable">There isn't a cast of the requested type set on the encoder.</exception>
        /// <exception cref="Errors.Timeout">The server did not respond to the request within the timeout period, as set with the <see cref="Bass.NetTimeOut"/> config option.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static string CastGetStats(int Handle, EncodeStats Type, string Password)
        {
            return Marshal.PtrToStringAnsi(BASS_Encode_CastGetStats(Handle, Type, Password));
        }

        [DllImport(DllName, EntryPoint = "BASS_Encode_CastInit")]
        public static extern bool CastInit(int handle,
            string server,
            string pass,
            string content,
            string name,
            string url,
            string genre,
            string desc,
            string headers,
            int bitrate,
            bool pub);

        [DllImport(DllName)]
        static extern bool BASS_Encode_CastSendMeta(int handle, EncodeMetaDataType type, byte[] data, int length);
        
        /// <summary>
        /// Sends metadata to a Shoutcast 2 server.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <param name="Type">The type of metadata.</param>
        /// <param name="Buffer">The XML metadata as an UTF-8 encoded byte array.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">There isn't a cast set on the encoder.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static bool CastSendMeta(int Handle, EncodeMetaDataType Type, byte[] Buffer)
        {
            return BASS_Encode_CastSendMeta(Handle, Type, Buffer, Buffer.Length);
        }

        /// <summary>
        /// Sends metadata to a Shoutcast 2 server.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <param name="Type">The type of metadata.</param>
        /// <param name="Metadata">The XML metadata to send.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">There isn't a cast set on the encoder.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static bool CastSendMeta(int Handle, EncodeMetaDataType Type, string Metadata)
        {
            if (string.IsNullOrEmpty(Metadata))
                return false;

            var bytes = Encoding.UTF8.GetBytes(Metadata);
            return BASS_Encode_CastSendMeta(Handle, Type, bytes, bytes.Length);
        }

        /// <summary>
        /// Sets the title (ANSI) of a cast stream.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <param name="Title">The title to set.</param>
        /// <param name="Url">URL to go with the title... <see langword="null" /> = no URL. This applies to Shoutcast only (not Shoutcast 2).</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The ISO-8859-1 (Latin-1) character set should be used with Shoutcast servers, and UTF-8 with Icecast and Shoutcast 2 servers.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">There isn't a cast set on the encoder.</exception>
        /// <exception cref="Errors.Timeout">The server did not respond to the request within the timeout period, as set with the <see cref="Bass.NetTimeOut"/> config option.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_CastSetTitle")]
        public static extern bool CastSetTitle(int Handle, string Title, string Url);

        /// <summary>
        /// Sets the title of a cast stream.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <param name="Title">encoded byte[] containing the title to set.</param>
        /// <param name="Url">encoded byte[] containing the URL to go with the title... <see langword="null" /> = no URL. This applies to Shoutcast only (not Shoutcast 2).</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The ISO-8859-1 (Latin-1) character set should be used with Shoutcast servers, and UTF-8 with Icecast and Shoutcast 2 servers.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">There isn't a cast set on the encoder.</exception>
        /// <exception cref="Errors.Timeout">The server did not respond to the request within the timeout period, as set with the <see cref="Bass.NetTimeOut"/> config option.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_CastSetTitle")]
        public static extern bool CastSetTitle(int Handle, byte[] Title, byte[] Url);
        #endregion

        #region Server
        [DllImport(DllName, EntryPoint = "BASS_Encode_ServerInit")]
        public static extern int ServerInit(int handle, string port, int buffer, int burst, EncodeServer flags, EncodeClientProcedure proc, IntPtr user);
        
		/// <summary>
		/// Kicks clients from a server.
		/// </summary>
		/// <param name="Handle">The encoder handle.</param>
		/// <param name="Client">The client(s) to kick... "" (empty string) = all clients. Unless a port number is included, this string is compared with the start of the connected clients' IP address.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
        /// The clients may not be kicked immediately, but shortly after the call.
        /// If the server has been setup with an <see cref="EncodeClientProcedure" /> callback function, that will receive notification of the disconnections.
        /// </para>
		/// <para><b>Platform-specific</b></para>
		/// <para>This function is not available on Windows CE.</para>
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">No matching clients were found.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_ServerKick")]
        public static extern int ServerKick(int Handle, string Client = "");
        #endregion
    }
}