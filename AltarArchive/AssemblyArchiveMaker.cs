using System;
using System.Linq;
using System.Reflection;
using System.IO;

namespace AltarArchive {
	public static class AssemblyArchiveMaker {
		public static void Generate(string path, string assemblyFolder = ".", bool excludeSelf = true) {
			var assemblies = from file in Directory.EnumerateFiles(assemblyFolder, "*.dll", SearchOption.TopDirectoryOnly)
							 let assemblyName = GetAssemblyName(file)
							 let selfName = excludeSelf ? typeof(AssemblyArchiveMaker).Assembly.GetName().Name : null
							 where assemblyName != null && assemblyName != selfName
							 select new ArchiveFile() {
								 FromFile = file,
								 Name = assemblyName
							 };
			ArchiveMaker.Generate(path, assemblies.ToArray());
		}

		private static string GetAssemblyName(string path) {
			try {
				return AssemblyName.GetAssemblyName(path).Name;
			} catch (BadImageFormatException) {
				return null;
			}
		}
	}
}
