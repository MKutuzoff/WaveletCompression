using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveletCompression {
	public class BitReader {

		private Stream _stream = null;
		private int _buffer = 0;
		private int _offset = 0;

		public long Position { get => _stream.Position; }

		public BitReader(Stream stream) {
			_stream = stream;
		}

		public bool EOF {
			get {
				return _stream.Position >= _stream.Length;
			}
		}

		public bool ReadBit() {
			if (!EOF) {
				if (_offset == 0) {
					_buffer = (_buffer << 8) & 0xffff;
					_offset = _buffer == 0xff00 ? 7 : 8;
					_buffer |= _stream.ReadByte();
				}
				--_offset;
				return ((_buffer >> _offset) & 1) == 1;
			}
			return false;
		}

		public int Read(int length) {
			int result = 0;
			for (var i = length - 1; i >= 0; --i) {
				if (ReadBit()) result |= 0x01 << i;
			}
			return result;
		}

		public long Move(long offset) {
			_offset = 0;
			return _stream.Seek(offset, SeekOrigin.Current);
		}

		public byte[] CopyByte(int length) {
			if (_stream.Position + length < _stream.Length) {
				byte[] data = new byte[length];
				_stream.Read(data, 0, length);
				_offset = 0;
				return data;
			} else {
				return null;
			}
		}

		public int ReadVLengthCode() {
			int n = 0;
			if (!ReadBit())
				return 1;
			else if (!ReadBit())
				return 2;
			else if ((n = Read(2)) != 3)
				return 3 + n;
			else if ((n = Read(5)) != 31)
				return 6 + n;
			return 37 + Read(7);
		}
	}
}
