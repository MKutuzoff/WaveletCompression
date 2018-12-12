using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveletCompression {
	public class BitReader {

		byte[] _data = null;
		int _buffer = 0;
		int _offset = 0;
		int _position = 0;

		public BitReader(byte[] data) {
			_data = data;
		}

		public bool EOF {
			get {
				return _position >= _data.Length;
			}
		}

		public bool ReadBit() {
			if (_data != null && _position < _data.Length) {
				if (_offset == 0) {
					_buffer = (_buffer << 8) & 0xffff;
					_offset = _buffer == 0xff00 ? 7 : 8;
					_buffer |= _data[_position++];
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

		public byte[] CopyByte(int length) {
			if (_position + length < _data.Length) {
				byte[] data = new byte[length];
				Array.Copy(_data, _position, data, 0, length);
				_offset = 0;
				_position += length;
				return data;
			} else {
				return null;
			}
		}
	}
}
