﻿using System.Runtime.InteropServices;
using System;

namespace ManagedBass.Fx
{
    /// <summary>
    /// Wraps BassFx: bassfx.dll
    /// </summary>
    public static partial class BassFx
    {
        #region Version
        [DllImport(DllName)]
        static extern int BASS_FX_GetVersion();

        /// <summary>
        /// Gets the Version of BassFx that is loaded.
        /// </summary>
        public static Version Version => Extensions.GetVersion(BASS_FX_GetVersion());
        #endregion
        
        [DllImport(DllName, EntryPoint = "BASS_FX_TempoCreate")]
        public static extern int TempoCreate(int Channel, BassFlags Flags);
        
		/// <summary>
		/// Get the source channel handle of the reversed stream.
		/// </summary>
		/// <param name="Channel">The handle of the reversed stream.</param>
		/// <returns>If successful, the handle of the source of the reversed stream is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Channel" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_FX_TempoGetSource")]
        public static extern int TempoGetSource(int Channel);
        
		/// <summary>
		/// Get the ratio of the resulting rate and source rate (the resampling ratio).
		/// </summary>
		/// <param name="Channel">Tempo stream (or source channel) handle.</param>
		/// <returns>If successful, the resampling ratio is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Channel" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_FX_TempoGetRateRatio")]
        public static extern float TempoGetRateRatio(int Channel);

        [DllImport(DllName, EntryPoint = "BASS_FX_ReverseCreate")]
        public static extern int ReverseCreate(int Channel, float DecodingBlockLength, BassFlags Flags);
        
		/// <summary>
		/// Get the source channel handle of the reversed stream.
		/// </summary>
		/// <param name="Channel">The handle of the reversed stream.</param>
		/// <returns>If successful, the handle of the source of the reversed stream is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Channel" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_FX_ReverseGetSource")]
        public static extern int ReverseGetSource(int Channel);

        #region BPM
        [DllImport(DllName, EntryPoint = "BASS_FX_BPM_DecodeGet")]
        public static extern float BPMDecodeGet(int Channel, double StartSec, double EndSec, int MinMaxBPM, BassFlags Flags, BPMProgressProcedure Procedure, IntPtr User = default(IntPtr));

        [DllImport(DllName, EntryPoint = "BASS_FX_BPM_CallbackSet")]
        public static extern bool BPMCallbackSet(int Handle, BPMProcedure Procedure, double Period, int MinMaxBPM, BassFlags Flags, IntPtr User = default(IntPtr));

        /// <summary>
        /// Reset the BPM buffers.
        /// </summary>
        /// <param name="Handle">Stream/music/wma/cd/any other supported add-on format.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// This function flushes the internal buffers of the BPM callback.
        /// The BPM callback is automatically reset by <see cref="Bass.ChannelSetPosition" />, except when called from a <see cref="SyncFlags.Mixtime"/> <see cref="SyncProcedure" />.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_FX_BPM_CallbackReset")]
        public static extern bool BPMCallbackReset(int Handle);
        
		/// <summary>
		/// Frees all resources used by a given handle.
		/// </summary>
		/// <param name="Handle">Stream/music/wma/cd/any other supported add-on format.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
        /// Used together with <see cref="BPMDecodeGet" /> or <see cref="BPMCallbackSet" />.
		/// If <see cref="BassFlags.FxFreeSource"/> was used, this will also free the underlying decoding channel as well.
		/// You can't set/get this flag with <see cref="Bass.ChannelFlags" />/<see cref="Bass.ChannelGetInfo(int, out ChannelInfo)" />.
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_FX_BPM_Free")]
        public static extern bool BPMFree(int Handle);

        [DllImport(DllName, EntryPoint="BASS_FX_BPM_BeatDecodeGet")]
        public static extern bool BPMBeatDecodeGet(int Channel, double StartSec, double EndSec, BassFlags Flags, BPMBeatProcedure Procedure, IntPtr User = default(IntPtr));

        [DllImport(DllName, EntryPoint="BASS_FX_BPM_BeatCallbackSet")]
        public static extern bool BPMBeatCallbackSet(int Handle, BPMBeatProcedure Procedure, IntPtr User = default(IntPtr));
        
		/// <summary>
		/// Reset the BPM buffers.
		/// </summary>
		/// <param name="Handle">Stream/music/wma/cd/any other supported add-on format.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// This function flushes the internal buffers of the BPM callback.
        /// The BPM callback is automatically reset by <see cref="Bass.ChannelSetPosition" />, except when called from a <see cref="SyncFlags.Mixtime"/> <see cref="SyncProcedure" />.
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint="BASS_FX_BPM_BeatCallbackReset")]
        public static extern bool BPMBeatCallbackReset(int Handle);

        [DllImport(DllName, EntryPoint="BASS_FX_BPM_BeatSetParameters")]
        public static extern bool BPMBeatSetParameters(int Handle, float Bandwidth, float CenterFreq, float Beat_rTime);

        [DllImport(DllName, EntryPoint="BASS_FX_BPM_BeatGetParameters")]
        public static extern bool BPMBeatGetParameters(int Handle, out float Bandwidth, out float CenterFreq, out float Beat_rTime);
        
		/// <summary>
		/// Free all resources used by a given handle (decode or callback beat).
		/// </summary>
		/// <param name="Handle">Stream/music/wma/cd/any other supported add-on format.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
        /// Used together with <see cref="BPMBeatDecodeGet" /> or <see cref="BPMBeatCallbackSet" />.
		/// <para>
        /// Note: If the <see cref="BassFlags.FxFreeSource"/> flag is used, this will free the source decoding channel as well.
        /// You can't set/get this flag with <see cref="Bass.ChannelFlags" /> and <see cref="Bass.ChannelGetInfo(int, out ChannelInfo)" />.
        /// </para>
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_FX_BPM_BeatFree")]
        public static extern bool BPMBeatFree(int Handle);
        #endregion
    }
}