﻿using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Dsd
{
    /// <summary>
    /// Wraps BassDsd
    /// </summary> 
    /// <remarks>
    /// Supports .dsf, .dff, .dsd
    /// </remarks>
    public static class BassDsd
    {
#if __IOS__
        const string DllName = "__Internal";
#else
        const string DllName = "bassdsd";
#endif

#if __ANDROID__ || WINDOWS || LINUX || __MAC__
        static IntPtr hLib;

        /// <summary>
        /// Load this library into Memory.
        /// </summary>
        /// <param name="Folder">Directory to Load from... <see langword="null"/> (default) = Load from Current Directory.</param>
        /// <returns><see langword="true" />, if the library loaded successfully, else <see langword="false" />.</returns>
        /// <remarks>
        /// <para>
        /// An external library is loaded into memory when any of its methods are called for the first time.
        /// This results in the first method call being slower than all subsequent calls.
        /// </para>
        /// <para>
        /// Some BASS libraries and add-ons may introduce new options to the main BASS lib like new parameters.
        /// But, before using these new options the respective library must be already loaded.
        /// This method can be used to make sure, that this library has been loaded.
        /// </para>
        /// </remarks>
        public static bool Load(string Folder = null) => (hLib = DynamicLibrary.Load(DllName, Folder)) != IntPtr.Zero;

        /// <summary>
        /// Unloads this library from Memory.
        /// </summary>
        /// <returns><see langword="true" />, if the library unloaded successfully, else <see langword="false" />.</returns>
        public static bool Unload() => DynamicLibrary.Unload(hLib);
#endif

        /// <summary>
        /// Use this library as a Plugin.
        /// </summary>
        public static readonly Plugin Plugin = new Plugin(DllName);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_DSD_StreamCreateFile(bool mem, string file, long offset, long length, BassFlags flags, int Frequency = 0);

        [DllImport(DllName)]
        static extern int BASS_DSD_StreamCreateFile(bool mem, IntPtr file, long offset, long length, BassFlags flags, int Frequency = 0);

        /// <summary>Create a stream from file.</summary>
        public static int CreateStream(string File, long Offset = 0, long Length = 0, BassFlags Flags = BassFlags.Default, int Frequency = 0)
        {
            return BASS_DSD_StreamCreateFile(false, File, Offset, Length, Flags | BassFlags.Unicode, Frequency);
        }

        /// <summary>Create a stream from Memory (IntPtr).</summary>
        public static int CreateStream(IntPtr Memory, long Offset, long Length, BassFlags Flags = BassFlags.Default, int Frequency = 0)
        {
            return BASS_DSD_StreamCreateFile(true, new IntPtr(Memory.ToInt64() + Offset), 0, Length, Flags, Frequency);
        }

        /// <summary>Create a stream from Memory (byte[]).</summary>
        public static int CreateStream(byte[] Memory, long Offset, long Length, BassFlags Flags, int Frequency = 0)
        {
            var GCPin = GCHandle.Alloc(Memory, GCHandleType.Pinned);

            var Handle = CreateStream(GCPin.AddrOfPinnedObject(), Offset, Length, Flags, Frequency);

            if (Handle == 0) GCPin.Free();
            else Bass.ChannelSetSync(Handle, SyncFlags.Free, 0, (a, b, c, d) => GCPin.Free());

            return Handle;
        }

        [DllImport(DllName)]
        static extern int BASS_DSD_StreamCreateFileUser(StreamSystem system, BassFlags flags, [In, Out] FileProcedures procs, IntPtr user, int Frequency = 0);

        /// <summary>Create a stream using User File Procedures.</summary>
        public static int CreateStream(StreamSystem System, BassFlags Flags, FileProcedures Procedures, IntPtr User = default(IntPtr), int Frequency = 0)
        {
            var h = BASS_DSD_StreamCreateFileUser(System, Flags, Procedures, User, Frequency);

            if (h != 0)
                Extensions.ChannelReferences.Add(h, 0, Procedures);

            return h;
        }

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_DSD_StreamCreateURL(string Url, int Offset, BassFlags Flags, DownloadProcedure Procedure, IntPtr User, int Frequency = 0);

        /// <summary>Create a stream from Url.</summary>
        public static int CreateStream(string Url, int Offset, BassFlags Flags, DownloadProcedure Procedure, IntPtr User = default(IntPtr), int Frequency = 0)
        {
            var h = BASS_DSD_StreamCreateURL(Url, Offset, Flags | BassFlags.Unicode, Procedure, User, Frequency);

            if (h != 0)
                Extensions.ChannelReferences.Add(h, 0, Procedure);

            return h;
        }

        /// <summary>
        /// The default sample rate when converting to PCM.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This setting determines what sample rate is used by default when converting to PCM.
        /// The rate actually used may be different if the specified rate is not valid for a particular DSD rate, in which case it will be rounded up (or down if there are none higher) to the nearest valid rate;
        /// the valid rates are 1/8, 1/16, 1/32, etc. of the DSD rate down to a minimum of 44100 Hz.
        /// </para>
        /// <para>
        /// The default setting is 88200 Hz.
        /// Changes only affect subsequently created streams, not any that already exist.
        /// </para>
        /// </remarks>
        public static int DefaultFrequency
        {
            get { return Bass.GetConfig(Configuration.DSDFrequency); }
            set { Bass.Configure(Configuration.DSDFrequency, value); }
        }

        /// <summary>
        /// The default gain applied when converting to PCM.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This setting determines what gain is applied by default when converting to PCM.
        /// Changes only affect subsequently created streams, not any that already exist.
        /// An existing stream's gain can be changed via the <see cref="ChannelAttribute.DSDGain" /> attribute.
        /// </para>
        /// <para>The default setting is 6dB.</para>
        /// </remarks>
        public static int DefaultGain
        {
            get { return Bass.GetConfig(Configuration.DSDGain); }
            set { Bass.Configure(Configuration.DSDGain, value); }
        }
    }
}