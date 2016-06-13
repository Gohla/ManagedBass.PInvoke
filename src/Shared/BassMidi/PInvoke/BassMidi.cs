﻿using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Midi
{
    /// <summary>
    /// Wraps BassMidi: bassmidi.dll
    /// 
    /// <para>Supports: .midi, .mid, .rmi, .kar</para>
    /// </summary>
    public static partial class BassMidi
    {
        const int BASS_MIDI_FONT_EX = 0x1000000;

        public const int ChorusChannel = -1,
                         ReverbChannel = -2,
                         UserFXChannel = -3;
        
        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamCreate")]
        public static extern int CreateStream(int Channels, BassFlags Flags = BassFlags.Default, int Frequency = 0);

        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamCreateEvents")]
        public static extern int CreateStream(MidiEvent[] events, int ppqn, BassFlags flags = BassFlags.Default, int freq = 0);
        
        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamEvent")]
        public static extern bool StreamEvent(int Handle, int chan, MidiEventType Event, int param);

        [DllImport(DllName, EntryPoint= "BASS_MIDI_StreamEvents")]
        public static extern int StreamEvents(int Handle, MidiEventsMode Mode, IntPtr Events, int Length);

        [DllImport(DllName)]
        static extern int BASS_MIDI_StreamEvents(int Handle, MidiEventsMode Mode, MidiEvent[] Events, int Length);

        [DllImport(DllName)]
        static extern int BASS_MIDI_StreamEvents(int Handle, MidiEventsMode Mode, byte[] Events, int Length);

        public static int StreamEvents(int Handle, MidiEventsMode Mode, MidiEvent[] Events, int Length = 0)
        {
            return BASS_MIDI_StreamEvents(Handle, Mode & ~MidiEventsMode.Raw, Events, Length == 0 ? Events.Length : Length);
        }

        public static int StreamEvents(int Handle, MidiEventsMode Mode, byte[] Raw, int Length = 0)
        {
            return BASS_MIDI_StreamEvents(Handle, MidiEventsMode.Raw | Mode, Raw, Length == 0 ? Raw.Length : Length);
        }

        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamGetChannel")]
        public static extern int StreamGetChannel(int handle, int chan);

        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamGetEvent")]
        public static extern int StreamGetEvent(int handle, int chan, MidiEventType Event);

        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamGetEvents")]
        public static extern int StreamGetEvents(int handle, int track, int filter, [In, Out] MidiEvent[] events);

        public static MidiEvent[] StreamGetEvents(int handle, int track, int filter)
        {
            var count = StreamGetEvents(handle, track, filter, null);

            if (count <= 0)
                return null;

            var events = new MidiEvent[count];

            StreamGetEvents(handle, track, filter, events);

            return events;
        }

        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamGetFonts")]
        public static extern int StreamGetFonts(int handle, IntPtr fonts, int count);
        
        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamGetFonts")]
        public static extern int StreamGetFonts(int handle, [In][Out] MidiFont[] fonts, int count);

        [DllImport(DllName)]
        static extern int BASS_MIDI_StreamGetFonts(int handle, [In][Out] MidiFontEx[] fonts, int count);
        
        public static int StreamGetFonts(int handle, MidiFontEx[] fonts, int count)
        {
            return BASS_MIDI_StreamGetFonts(handle, fonts, count | BASS_MIDI_FONT_EX);
        }

        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamGetMark")]
        public static extern bool StreamGetMark(int handle, MidiMarkerType type, int index, out MidiMarker mark);

        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamGetMarks")]
        public static extern int StreamGetMarks(int handle, int track, MidiMarkerType type, [In, Out] MidiMarker[] marks);

        public static MidiMarker[] StreamGetMarks(int handle)
        {
            var markCount = StreamGetMarks(handle, -1, MidiMarkerType.Marker, null);

            if (markCount <= 0)
                return null;

            var marks = new MidiMarker[markCount];
            StreamGetMarks(handle, -1, MidiMarkerType.Marker, marks);

            return marks;
        }
        
		/// <summary>
		/// Preloads the samples required by a MIDI file stream.
		/// </summary>
		/// <param name="Handle">The MIDI stream handle.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
        /// Samples are normally loaded as they are needed while rendering a MIDI stream, which can result in CPU spikes, particularly with packed soundfonts.
        /// That generally won't cause any problems, but when smooth/constant performance is critical this function can be used to preload the samples before rendering, so avoiding the need to load them while rendering.
        /// </para>
		/// <para>Preloaded samples can be compacted/unloaded just like any other samples, so it is probably wise to disable the <see cref="Compact"/> option when preloading samples, to avoid any chance of the samples subsequently being automatically unloaded.</para>
		/// <para>This function should not be used while the MIDI stream is being rendered, as that could interrupt the rendering.</para>
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">The stream is for real-time events only, so it's not possible to know what presets are going to be used. Use <see cref="FontLoad" /> instead.</exception>
        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamLoadSamples")]
        public static extern bool StreamLoadSamples(int Handle);

        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamSetFonts")]
        public static extern int StreamSetFonts(int handle, MidiFont[] fonts, int count);

        [DllImport(DllName)]
        static extern int BASS_MIDI_StreamSetFonts(int handle, MidiFontEx[] fonts, int count);

        public static int StreamSetFonts(int handle, MidiFontEx[] fonts, int count)
        {
            return BASS_MIDI_StreamSetFonts(handle, fonts, count | BASS_MIDI_FONT_EX);
        }
    }
}