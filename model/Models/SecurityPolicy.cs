using System.Text;

namespace SchemaZen.Library.Models
{
	public class SecurityPolicy :INameable, IScriptable
	{
		public string Name { get; set; }
		public string TableSchema { get; set; }
		public string TableName { get; set; }
		public string PredicateType { get; set; }
		public string PredicateDefinition { get; set; }
		public bool IsEnabled { get; set; }
		public bool IsSchemaBound { get; set; }

		public SecurityPolicy(string name, string tableSchema, string tableName, string predicateType, string predicateDefinition, bool isEnabled, bool isSchemaBound)
		{
			Name = name;
			TableSchema = tableSchema;
			TableName = tableName;
			PredicateType = predicateType;
			PredicateDefinition = predicateDefinition;
			IsEnabled = isEnabled;
			IsSchemaBound = isSchemaBound;
		}

		private string ToOnOff(bool b)
		{
			return b ? "ON" : "OFF";
		}

		public string ScriptCreate()
		{
			var text = new StringBuilder();
			text.Append($"CREATE SECURITY POLICY [dbo].[{Name}]\r\n");
			text.Append($"ADD {PredicateType} PREDICATE {PredicateDefinition}\r\n");
			text.Append($"ON [{TableSchema}].[{TableName}]\r\n");
			text.Append($"WITH (STATE = {ToOnOff(IsEnabled)}, SCHEMABINDING = {ToOnOff(IsSchemaBound)})\r\n");

			return text.ToString();
		}

		public string ScriptDrop()
		{
			return $"DROP SECURITY POLICY IF EXISTS [dbo].[{Name}]\r\n";
		}
	}
}
