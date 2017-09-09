using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace AltarArchive {
	public class ArchiveFile {
		public Stream FromStream { get; set; }
		public long FromStreamIndex { get; set; }
		public long FromStreamLength { get; set; }
		public string FromFile { get; set; }
		public string Name { get; set; }

		internal string FileName;
		internal long FileLength;
	}

	public static class ArchiveMaker {
		public static void Generate(string outputFile, params string[] files) {
			Generate(outputFile, files.Select(file => new ArchiveFile() { FromFile = file }).ToArray());
		}
		public static void Generate(string outputFile, params Stream[] files) {
			Generate(outputFile, files.Select(file => new ArchiveFile() { FromStream = file }).ToArray());
		}
		public static void Generate(string outputFile, params ArchiveFile[] files) {
			using (var fs = new FileStream(outputFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
				Generate(fs, files);
		}
		public static void Generate(Stream output, params string[] files) {
			Generate(output, files.Select(file => new ArchiveFile() { FromFile = file }).ToArray());
		}
		public static void Generate(Stream output, params Stream[] files) {
			Generate(output, files.Select(file => new ArchiveFile() { FromStream = file }).ToArray());
		}
		public static void Generate(Stream output, params ArchiveFile[] files) {
			// First we validate the files we wish to add to the archive
			foreach (var file in files) {
				if (file.FromStream == null && file.FromFile != null) {
					if (File.Exists(file.FromFile) == false)
						throw new FileNotFoundException(file.FromFile);
					if (File.GetAttributes(file.FromFile).HasFlag(FileAttributes.Directory))
						throw new IOException("Cannot add a folder to the archive. Select individual files instead.");
				} else if (file.FromStream != null && file.Name != null) {
					file.FileLength = file.FromStreamLength <= 0 ? file.FromStream.Length : file.FromStreamLength;
					if (file.FromStreamIndex == 0)
						file.FromStreamIndex = file.FromStream.Position;
				}
				else throw new IOException("The FromStream OR FromFile property must be set, and the Name property is not optional if the FromStream property is set.");
				// We also set some values while we're at it
				if (file.FromFile != null) {
					file.FileName = Path.GetFileName(file.FromFile);
					file.FileLength = new FileInfo(file.FromFile).Length;
                    if (file.Name == null)
						file.Name = file.FileName;
				}
			}
			// Check for duplicate names
			if (files.GroupBy(x => x.Name).Where(g => g.Count() > 1).Any())
				throw new IOException("Duplicate names given at archive generation.");

			// currently unused, we can have some boolean flags (8 of them) in a single byte, as potential options
			var barray = new BitArray(8);
			// barray[0] = insert_boolean_option_here;
			var BABuffer = new byte[1];
			barray.CopyTo(BABuffer, 0);
			var options = BABuffer[0];

			// We do a first pass to create the archive header, with names and file length
			long headerEnd = 0;
			using (var bw = new BinaryWriter(output, Encoding.UTF8, true)) {
				bw.Write(Archive.Version);
				bw.Write(options);
				foreach (var file in files) {
					bw.Write((byte)1);
					bw.Write(file.Name);
					bw.Write((long)0);
					bw.Write(file.FileLength);
				}
				bw.Write((byte)0);
			}
			output.Flush();
			headerEnd = output.Position;

			// Then a second and final pass to properly set the starting position of each files within
			output.Seek(0, SeekOrigin.Begin);
			using (var br = new BinaryReader(output, Encoding.UTF8, true))
				using (var bw = new BinaryWriter(output, Encoding.UTF8, true)) {
					long pos = headerEnd;
					byte type = 0;
					br.ReadByte();
					br.ReadByte();
					while ((type = br.ReadByte()) != 0) {
						br.ReadString();
						bw.Write(pos);
						pos += br.ReadInt64();
                    }
				}

			// Finally, we add the files
			output.Seek(0, SeekOrigin.End);
			foreach (var file in files) {
				if (file.FromStream != null) {
					var section = new SubStream(file.FromStream, null, file.FromStreamIndex, file.FromStreamLength);
					section.CopyTo(output);
				}
				else using (var fileFs = new FileStream(file.FromFile, FileMode.Open, FileAccess.Read, FileShare.Read))
						fileFs.CopyTo(output);
			}
		}
	}
}
