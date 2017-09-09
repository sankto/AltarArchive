using System;
using System.IO;

namespace AltarArchive {
	public class SubStream : Stream {
		private readonly Stream MainStream;
		private readonly long StartIndex;
		private readonly long SubStreamLength;
		private long SubStreamPosition;
		public readonly string Name;

		internal SubStream(Stream stream, string name, long start, long length) : base() {
			MainStream = stream;
			StartIndex = start;
			SubStreamLength = length;
			SubStreamPosition = 0;
			Name = name;
		}

		public override bool CanRead { get { return true; } }
		public override bool CanSeek { get { return true; } }
		public override bool CanWrite { get { return true; } }
		public override long Length { get { return SubStreamLength; } }

		public override long Position {
			get {
				return SubStreamPosition;
			}
			set {
				SubStreamPosition = value;
			}
		}

		public override void Flush() { }

		public override int Read(byte[] buffer, int offset, int count) {
			if (count <= 0)
				return 0;
			SeekIfNotCurrent();
			if (Position + count > Length)
				count = (int)(Length - Position);
			var read = count <= 0 ? 0 : MainStream.Read(buffer, offset, count);
			Position += read;
			return read;
		}

		public override long Seek(long offset, SeekOrigin origin) {
			SeekIfNotCurrent(origin);
			if (Position + offset < 0 || Position + offset >= Length)
				throw new IndexOutOfRangeException("Cannot seek outside of boundaries.");
			Position = MainStream.Seek(offset, SeekOrigin.Current) - StartIndex;
			return Position;
		}

		public override void SetLength(long value) {
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count) {
			if (count <= 0)
				return;
			SeekIfNotCurrent();
			if (Position + count > Length)
				throw new IndexOutOfRangeException("Cannot write outside of boundaries.");
			MainStream.Write(buffer, offset, count);
			Position = MainStream.Position - StartIndex;
		}

		private void SeekIfNotCurrent(SeekOrigin origin = SeekOrigin.Current) {
			switch (origin) {
				case SeekOrigin.Current:
					if (MainStream.Position != StartIndex + Position)
						Position = MainStream.Seek(StartIndex + Position, SeekOrigin.Begin) - StartIndex;
					break;
				case SeekOrigin.Begin:
					if (MainStream.Position != StartIndex)
						Position = MainStream.Seek(StartIndex, SeekOrigin.Begin) - StartIndex;
					break;
				case SeekOrigin.End:
					if (MainStream.Position != StartIndex + Length - 1)
						Position = MainStream.Seek(StartIndex + Length - 1, SeekOrigin.Begin) - StartIndex;
					break;
			}
		}
	}
}
