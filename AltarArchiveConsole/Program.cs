using System;
using AltarArchive;
using Newtonsoft.Json;

namespace AltarArchiveConsole {
	class Program {
		static void Main(string[] args) {
			try {
				//AssemblyArchiveMaker.Generate("asm.pak");

				AssemblyArchive.StartResolving("asm.pak");

				Test();

				/*ArchiveMaker.Generate("arc.aa", "musics/1.mp3", "musics/2.mp3", "musics/3.mp3");
				using (var reader = new Archive("arc.aa")) {
					using (var fs = File.Create("test2.mp3"))
						reader["2.mp3"].CopyTo(fs);
					using (var fs = File.Create("test3.mp3"))
						reader["3.mp3"].CopyTo(fs);
					using (var fs = File.Create("test1.mp3"))
						reader["1.mp3"].CopyTo(fs);
				}

				using (var readFs = new FileStream("musics/1.mp3", FileMode.Open, FileAccess.Read, FileShare.Read))
					ArchiveMaker.Generate("arc2.aa",
						new ArchiveFile() {
							FromStream = readFs,
							//FromFile = "musics/1.mp3",
							Name = "test",
							FromStreamLength = 16 * 1024,
							FromStreamIndex = 500
						}
					);
				using (var reader = new Archive("arc2.aa")) {
					using (var fs = File.Create("test1_2.mp3"))
						reader["test"].CopyTo(fs);
				}*/
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
			}
			Console.WriteLine("-------------------------------");
			Console.ReadKey();
		}

		private static void Test() {
			var asdf = new JsonArrayAttribute("5");
		}
	}
}
