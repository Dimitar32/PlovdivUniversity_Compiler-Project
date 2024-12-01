using NUnit.Framework;
using scsc;
using System;
using System.Diagnostics;
using System.IO;

namespace MTProjectTests
{
    public class Tests
	{
		protected static string TestCasesDir
			   = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
							  Path.Combine("..",
											Path.Combine("..", "TestCases" + Path.DirectorySeparatorChar)));

		private static string Normalize(string s)
		{
			char[] CharsToTrim = new char[] { ' ', '\t' };
			// for Mac, Win, Lin
			return s.Normalize().Replace("\n\r", "\n").Replace("\r\n", "\n").Replace("\r", "\n").Trim(CharsToTrim);
		}

		private static void RunTestMethod(string testname)
		{
			string testPath = TestCasesDir + testname;
			string testCasesTmpDir = TestCasesDir + "tmp" + Path.DirectorySeparatorChar;
			string sourceFilePath = testPath + ".scs";
			string exeFilePath = testCasesTmpDir + testname + ".exe";
			string resultFilePath = testPath + ".result";

			// Replace the default diagnostic client with our new custom one.
			// VerifyDiagnostics analyzes special syntax hidden in the comments
			// and verifies whether an error was expected or not.
			VerifyDiagnostics diag = new VerifyDiagnostics();
			bool Compiled = Compiler.Compile(sourceFilePath, exeFilePath, diag);
			Assert.IsTrue(Compiled);

			// If the compilation was fine we have an assembly.
			// If there is a testname.result file we run the assembly and 
			// compare the output of the assembly and the expected output
			// in the .result file.
			if (File.Exists(resultFilePath))
			{
				string resultFileText = Normalize(File.ReadAllText(resultFilePath));
				Process p = new Process();
				p.StartInfo.FileName = exeFilePath;
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.RedirectStandardError = true;
				p.StartInfo.ErrorDialog = false;

				p.Start();
				if (!p.WaitForExit(15000))
					p.Kill();

				string output = Normalize(p.StandardOutput.ReadToEnd());
				string error = Normalize(p.StandardError.ReadToEnd());

				Assert.AreEqual(resultFileText, output, "Mismatch! Output was not expected:\n");
			}
		}

		[Test]
		public void EmptyProgram()
		{
			RunTestMethod("EmptyProgram");
		}

		[Test]
		public void AddingNumbers()
		{
			RunTestMethod("AddingNumbers");
		}
		[Test]
		public void SubtractingNumbers()
		{
			RunTestMethod("SubtractingNumbers");
		}
		[Test]
		public void MultiplyingNumbers()
		{
			RunTestMethod("MultiplyingNumbers");
		}
		[Test]
		public void DividingNumbers()
		{
			RunTestMethod("DivdingNumbers");
		}
	}
}
