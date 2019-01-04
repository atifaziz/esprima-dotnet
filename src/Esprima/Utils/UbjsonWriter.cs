using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Esprima.Utils
{
    /// <summary>
    /// A <see cref="JsonWriter"/> implementation that encodes JSON in the
    /// Universal Binary JSON (http://ubjson.org/) format.
    /// </summary>

    public sealed class UbjsonWriter : JsonWriter
    {
        private static readonly UTF8Encoding Utf8BomlessEncoding =
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        private byte[] _buffer;
        private readonly Stream _stream;

        public UbjsonWriter(Stream stream) =>
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));

        private byte[] GetBuffer(int desiredCapacity)
        {
            if (desiredCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(desiredCapacity));
            }

            if (desiredCapacity > (_buffer?.Length ?? 0))
            {
                Array.Resize(ref _buffer, Math.Max(desiredCapacity, 256));
            }

            Debug.Assert(_buffer != null);
            return _buffer;
        }

        private void Write(ValueType type) =>
            WritePrefix(type, 0);
        private void Write(ValueType type, byte a) =>
            WritePrefix(type, 1, a);
        private void Write(ValueType type, byte a, byte b) =>
            WritePrefix(type, 2, a, b);
        private void Write(ValueType type, byte a, byte b, byte c) =>
            WritePrefix(type, 3, a, b, c);
        private void Write(ValueType type, byte a, byte b, byte c, byte d) =>
            WritePrefix(type, 4, a, b, c, d);
        private void Write(ValueType type, byte a, byte b, byte c, byte d, byte e) =>
            WritePrefix(type, 5, a, b, c, d, e);
        private void Write(ValueType type, byte a, byte b, byte c, byte d, byte e, byte f) =>
            WritePrefix(type, 6, a, b, c, d, e, f);
        private void Write(ValueType type, byte a, byte b, byte c, byte d, byte e, byte f, byte g) =>
            WritePrefix(type, 7, a, b, c, d, e, f, g);
        private void Write(ValueType type, byte a, byte b, byte c, byte d, byte e, byte f, byte g, byte h) =>
            WritePrefix(type, 8, a, b, c, d, e, f, g, h);

        private void WritePrefix(ValueType type,
            int length,
            byte a = 0, byte b = 0, byte c = 0, byte d = 0,
            byte e = 0, byte f = 0, byte g = 0, byte h = 0)
        {
            length++; // +1 byte for type
            var buffer = GetBuffer(length);
            buffer[0] = unchecked((byte) type);
            if (length < 2) { goto write; } buffer[1] = a;
            if (length < 3) { goto write; } buffer[2] = b;
            if (length < 4) { goto write; } buffer[3] = c;
            if (length < 5) { goto write; } buffer[4] = d;
            if (length < 6) { goto write; } buffer[5] = e;
            if (length < 7) { goto write; } buffer[6] = f;
            if (length < 8) { goto write; } buffer[7] = g;
            if (length < 9) { goto write; } buffer[8] = h;
            write:
            _stream.Write(buffer, 0, length);
        }

        private enum ValueType
        {
            Null    = 'Z',
            True    = 'T',
            False   = 'F',
            Int32   = 'l',
            Int64   = 'L',
            Float64 = 'D',
            Char    = 'C',
            String  = 'S',
        }

        public override void Null() => Write(ValueType.Null);

        public override void Number(long n)
        {
            if (n <= int.MaxValue)
            {
                Int32Buffer i = unchecked((int) n);
                Write(ValueType.Int32, i.Byte0, i.Byte1, i.Byte2, i.Byte3);
            }
            else
            {
                Int64Buffer l = n;
                Write(ValueType.Int64, l.Byte0, l.Byte1, l.Byte2, l.Byte3,
                                       l.Byte4, l.Byte5, l.Byte6, l.Byte7);
            }
        }

        public override void Number(double n)
        {
            if (double.IsInfinity(n))
            {
                // Per spec:
                //
                // > Numeric values of infinity are encoded as a null value.
                // >
                // > Source: http://ubjson.org/type-reference/value-types/#numeric-infinity

                Null();
            }
            else
            {
                DoubleBuffer d = n;
                Write(ValueType.Float64, d.Byte0, d.Byte1, d.Byte2, d.Byte3,
                                         d.Byte4, d.Byte5, d.Byte6, d.Byte7);
            }
        }

        public override void String(string value)
        {
            char ch;
            if (value.Length == 1 && (ch = value[0]) < 0x80)
            {
                Write(ValueType.Char, unchecked((byte) ch));
            }
            else
            {
                Write(ValueType.String);
                var byteLength = Utf8BomlessEncoding.GetByteCount(value);
                Number(byteLength);
                var buffer = GetBuffer(byteLength);
                var encodedLength = Utf8BomlessEncoding.GetBytes(value, 0, value.Length, buffer, buffer.Length);
                Debug.Assert(encodedLength == byteLength);
                _stream.Write(buffer, 0, encodedLength);
            }
        }

        public override void Boolean(bool flag) => Write(flag ? ValueType.True : ValueType.False);

        private enum ContainerDelimiterType
        {
            ObjectStart = '{',
            ObjectEnd   = '}',
            ArrayStart  = '[',
            ArrayEnd    = ']',
        }

        private void Write(ContainerDelimiterType delimiter)
        {
            var buffer = GetBuffer(1);
            buffer[0] = unchecked((byte) delimiter);
            _stream.Write(buffer, 0, 1);
        }

        public override void StartArray()  => Write(ContainerDelimiterType.ArrayStart);
        public override void EndArray()    => Write(ContainerDelimiterType.ArrayEnd);

        public override void StartObject() => Write(ContainerDelimiterType.ObjectStart);
        public override void EndObject()   => Write(ContainerDelimiterType.ObjectEnd);

        public override void Member(string name) => String(name);

        [StructLayout(LayoutKind.Explicit)]
        private struct Int32Buffer
        {
            [FieldOffset(0)] public int Value;
            [FieldOffset(0)] public byte Byte0;
            [FieldOffset(1)] public byte Byte1;
            [FieldOffset(2)] public byte Byte2;
            [FieldOffset(3)] public byte Byte3;

            public Int32Buffer(int value) : this() => Value = value;

            public static implicit operator int(Int32Buffer buffer) => buffer.Value;
            public static implicit operator Int32Buffer(int value) => new Int32Buffer(value);
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct Int64Buffer
        {
            [FieldOffset(0)] public long Value;
            [FieldOffset(0)] public byte Byte0;
            [FieldOffset(1)] public byte Byte1;
            [FieldOffset(2)] public byte Byte2;
            [FieldOffset(3)] public byte Byte3;
            [FieldOffset(4)] public byte Byte4;
            [FieldOffset(5)] public byte Byte5;
            [FieldOffset(6)] public byte Byte6;
            [FieldOffset(7)] public byte Byte7;

            public Int64Buffer(long value) : this() => Value = value;

            public static implicit operator long(Int64Buffer buffer) => buffer.Value;
            public static implicit operator Int64Buffer(long value) => new Int64Buffer(value);
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct DoubleBuffer
        {
            [FieldOffset(0)] public double Value;
            [FieldOffset(0)] public byte Byte0;
            [FieldOffset(1)] public byte Byte1;
            [FieldOffset(2)] public byte Byte2;
            [FieldOffset(3)] public byte Byte3;
            [FieldOffset(4)] public byte Byte4;
            [FieldOffset(5)] public byte Byte5;
            [FieldOffset(6)] public byte Byte6;
            [FieldOffset(7)] public byte Byte7;

            public DoubleBuffer(double value) : this() => Value = value;

            public static implicit operator double(DoubleBuffer buffer) => buffer.Value;
            public static implicit operator DoubleBuffer(double value) => new DoubleBuffer(value);
        }
    }
}
