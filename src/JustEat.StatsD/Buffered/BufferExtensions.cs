using System;
using System.Buffers.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace JustEat.StatsD.Buffered
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class BufferExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteBytes(this ref Buffer src, Span<byte> destination)
        {
            if (destination.Length > src.Tail.Length)
            {
                return false;
            }

            destination.CopyTo(src.Tail);
            src.Tail = src.Tail.Slice(destination.Length);
            src.Written += destination.Length;
            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteUtf8String(this ref Buffer src, string str)
        {
#if NETSTANDARD2_0 || NET461
            var bucketBytes = Encoding.UTF8.GetBytes(str);
            return src.TryWriteBytes(bucketBytes);
#else
            int written = 0;
            try
            {
                written = Encoding.UTF8.GetBytes(str, src.Tail);
            }
#pragma warning disable CA1031
            catch (ArgumentException)
#pragma warning restore CA1031
            {
                return false;
            }

            src.Tail = src.Tail.Slice(written);
            src.Written += written;
            return true;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteByte(this ref Buffer src, byte ch)
        {
            const int OneByte = 1;
            if (src.Tail.Length < OneByte)
            {
                return false;
            }

            src.Tail[0] = ch;
            src.Tail = src.Tail.Slice(OneByte);
            src.Written += OneByte;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteBytes(this ref Buffer src, byte ch1, byte ch2)
        {
            const int TwoBytes = 2;
            if (src.Tail.Length < TwoBytes)
            {
                return false;
            }

            src.Tail[0] = ch1;
            src.Tail[1] = ch2;
            src.Tail = src.Tail.Slice(TwoBytes);
            src.Written += TwoBytes;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteBytes(this ref Buffer src, byte ch1, byte ch2, byte ch3)
        {
            const int ThreeBytes = 3;
            if (src.Tail.Length < ThreeBytes)
            {
                return false;
            }

            src.Tail[0] = ch1;
            src.Tail[1] = ch2;
            src.Tail[2] = ch3;
            src.Tail = src.Tail.Slice(ThreeBytes);
            src.Written += ThreeBytes;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteInt64(this ref Buffer src, long val)
        {
            if (!Utf8Formatter.TryFormat(val, src.Tail, out var consumed))
            {
                return false;
            }

            src.Tail = src.Tail.Slice(consumed);
            src.Written += consumed;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteDouble(this ref Buffer src, double val)
        {
            if (!Utf8Formatter.TryFormat((decimal)val, src.Tail, out var consumed))
            {
                return false;
            }

            src.Tail = src.Tail.Slice(consumed);
            src.Written += consumed;

            return true;
        }
    }
}
