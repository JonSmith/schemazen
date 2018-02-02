using SchemaZen.Library.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SchemaZen.Library.Command
{
	public class ScriptConstraingRenamingCommand : BaseCommand
	{
		public string Source { get; set; }
		public string OutFile { get; set; }


		public void Execute()
		{
			var db = new Database();
			db.Connection = Source;
			db.Load();

			if (!string.IsNullOrEmpty(OutFile))
			{
				if (!Overwrite && File.Exists(OutFile))
				{
					var message = $"{OutFile} already exists - set overwrite to true if you want to delete it";
					throw new InvalidOperationException(message);
				}
				File.WriteAllText(OutFile, ConstraintRenameScript(db));
				Console.WriteLine($"Script to rename constraints has been created at {Path.GetFullPath(OutFile)}");
			}
		}

		private string ConstraintRenameScript(Database db)
		{
			var sb = new StringBuilder();

			foreach (var table in db.Tables.OrderBy(t => t.Owner).ThenBy(t => t.Name))
			{
				var renameScripts = table.ScriptConstraintRename();
				if (renameScripts.Any())
				{
					// a GO line is needed between each rename
					foreach (var script in renameScripts)
					{
						sb.AppendLine(script);
						sb.AppendLine("GO");
					}
				}
			}

			return sb.ToString();
		}
	}
}
