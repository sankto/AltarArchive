using System;
using System.Reflection;

namespace AltarArchive {
	public static class AssemblyArchive {
		private static Archive Assemblies;
		public static bool IsResolving { get; private set; }

		public static void StartResolving(string path) {
			if (IsResolving)
				Assemblies.Dispose();
			Assemblies = new Archive(path);
			if (!IsResolving) {
				AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
				AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
				IsResolving = true;
			}
		}

		public static void StopResolving() {
			if (IsResolving) {
				Assemblies.Dispose();
				AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
				AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
				IsResolving = false;
			}
		}

		private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
			var an = new AssemblyName(args.Name);
			byte[] data = new byte[Assemblies[an.Name].Length];
			Assemblies[an.Name].Read(data, 0, data.Length);
			return AppDomain.CurrentDomain.Load(data);
		}

		private static void CurrentDomain_ProcessExit(object sender, EventArgs e) {
			if (Assemblies != null)
				Assemblies.Dispose();
		}
	}
}
