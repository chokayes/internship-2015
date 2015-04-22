using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kontur.Courses.Testing.Implementations;
using NUnit.Framework;

namespace Kontur.Courses.Testing
{
	class Program
	{
		static void Main()
		{
			if (!CheckTests()) return;
			var implementations = GetImplementations();
			CheckIncorrectImplementationsFail(implementations);
		}

		private static void CheckIncorrectImplementationsFail(IEnumerable<Type> implementations)
		{
			foreach (var implementation in implementations)
			{
				var isCorrectImplementation = implementation == typeof (WordsStatistics_CorrectImplementation);
				var failed = GetFailedTests(implementation, isCorrectImplementation).ToList();
				Console.Write(implementation.Name + "\t");
				if (failed.Any())
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("fails on tests: " + string.Join(", ", failed));
					Console.ForegroundColor = ConsoleColor.Gray;
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("pass all tests :(");
					Console.ForegroundColor = ConsoleColor.Gray;
				}
			}
		}

		private static IEnumerable<Type> GetImplementations()
		{
			return 
				Assembly.GetExecutingAssembly().GetTypes()
				.Where(typeof (IWordsStatistics).IsAssignableFrom)
				.Where(t => !t.IsAbstract && !t.IsInterface)
				.Where(t => t != typeof(WordsStatistics_CorrectImplementation));
		}

		private static bool CheckTests()
		{
			Console.WriteLine("Check all tests pass with correct implementation...");
			var failed = GetFailedTests(typeof(WordsStatistics_CorrectImplementation), true).ToList();
			if (failed.Any())
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Incorrect tests detected: " + string.Join(", ", failed));
				Console.ForegroundColor = ConsoleColor.Gray;
				return false;
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Tests are OK!");
				Console.WriteLine();
				Console.ForegroundColor = ConsoleColor.Gray;
				return true;
			}
		}

		private static IEnumerable<string> GetFailedTests(Type implementationType, bool printError)
		{
			foreach (var testMethod in GetTestMethods())
			{
				var impl = (IWordsStatistics)Activator.CreateInstance(implementationType);
				if (!RunTestMethod(impl, testMethod, printError))
					yield return testMethod.Name;
			}
		}

		private static bool RunTestMethod(IWordsStatistics impl, MethodInfo testMethod, bool printError)
		{
			var testObj = new WordsStatistics_Tests {stat = impl};
			try
			{
				testMethod.Invoke(testObj, new object[0]);
			}
			catch (Exception e)
			{
				if (printError)
					Console.WriteLine(e);
				return false;
			}
			return true;
		}

		private static IEnumerable<MethodInfo> GetTestMethods()
		{
			var testMethods = typeof(WordsStatistics_Tests).GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.Where(m => m.GetCustomAttribute<TestAttribute>() != null);
			return testMethods;
		}
	}
}
