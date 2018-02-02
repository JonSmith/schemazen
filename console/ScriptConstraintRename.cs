using System;
using System.IO;
using ManyConsole;
using NDesk.Options;
using SchemaZen.Library.Command;

namespace SchemaZen.console
{
	internal class ScriptConstraintRename : ConsoleCommand
	{
		private string _source;
		private string _outFile;
		private bool _overwrite;
		private bool _verbose;

		public ScriptConstraintRename()
		{
			IsCommand("ScriptConstraintRename", "Create a rename script for all table constraings to conform to standards.");
			Options = new OptionSet();
			SkipsCommandSummaryBeforeRunning();
			HasRequiredOption(
				"s|source=",
				"Connection string to a database to script.",
				o => _source = o);
			HasOption(
				"outFile=",
				"Create a sql script file in the specified path.",
				o => _outFile = o);
			HasOption(
				"o|overwrite",
				"Overwrite existing target without prompt.",
				o => _overwrite = o != null);
		}

		public override int Run(string[] remainingArguments)
		{
			if (!string.IsNullOrEmpty(_outFile))
			{
				Console.WriteLine();
				if (!_overwrite && File.Exists(_outFile))
				{
					var question = $"{_outFile} already exists - do you want to replace it";
					if (!ConsoleQuestion.AskYN(question))
					{
						return 1;
					}
				}
			}

			var command = new ScriptConstraingRenamingCommand
			{
				Source = _source,
				OutFile = _outFile,
				Overwrite = _overwrite,
			};

			try
			{
				command.Execute();
				return 1;
			}
			catch (Exception ex)
			{
				throw new ConsoleHelpAsException(ex.Message);
			}
		}
	}
}
