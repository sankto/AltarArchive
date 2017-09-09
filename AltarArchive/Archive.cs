using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AltarArchive {
	public class Archive : IDisposable {
		internal const byte Version = 0;
		private readonly Dictionary<string, SubStream> Subs;
		private FileStream MainStream;
		public readonly string FilePath;

		public Archive(string path) {
			FilePath = path;
			try {
				MainStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024);
			} catch {
				throw new IOException("The file '" + FilePath + "' could not be opened for reading.");
			}
			Subs = new Dictionary<string, SubStream>();
			try {
				using (var br = new BinaryReader(MainStream, Encoding.UTF8, true)) {
					var version = br.ReadByte();
					if (Version < version)
						throw new NotSupportedException("The ArchiveReader version (" + version + ") is inferior to the archive's version (" + version + ").");

					var options = br.ReadByte();
					var barray = new BitArray(new byte[] { options });
					//var get_boolean_option_here = barray[0];

					byte type = 0;
					while ((type = br.ReadByte()) != 0) {
						if (type == 1) { // is subStream
							var name = br.ReadString();
							Subs.Add(name, new SubStream(MainStream, name, br.ReadInt64(), br.ReadInt64()));
						}
					}
				}
			} catch (Exception e) {
				Dispose();
				throw new IOException("An error occurred while reading the header of the AltarArchive : " + e.Message);
			}
		}

		public Dictionary<string, SubStream>.KeyCollection Keys {
			get { return Subs.Keys; }
		}

		public Dictionary<string, SubStream>.ValueCollection Values {
			get { return Subs.Values; }
		}

		public SubStream this[string name] {
			get { return Subs[name]; }
		}

		public bool Contains(string name) {
			return Subs.ContainsKey(name);
		}

		public bool Contains(SubStream file) {
			return Subs.ContainsValue(file);
		}

		public void Dispose() {
			if (MainStream != null) {
				MainStream.Dispose();
				MainStream = null;
			}
		}

		~Archive() {
			Dispose();
		}
	}
}
