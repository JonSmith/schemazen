using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;

namespace SchemaZen.Library.Models {
	public class ObjectPermission
	{
		public string State { get; set; }
		public string StateDescription { get; set; }
		public string PermissionName { get; set; }
		public string ObjectSchema { get; set; }
		public string ObjectName { get; set; }
		public string ColumnName { get; set; }

		public string ScriptCreate(string userName)
		{
			var sb = new StringBuilder();
			if (State != "W")   // W=Grant With Grant Option
				sb.Append(StateDescription);
			else
				sb.Append("GRANT");
			sb.Append($" {PermissionName} ON {ObjectSchema}.{ObjectName}");
			if (!string.IsNullOrEmpty(ColumnName))
				sb.Append($"({ColumnName})");
			sb.Append($" TO {userName}");
			if (State == "W")
				sb.Append(" WITH GRANT OPTION");

			return sb.ToString();
		}
	}

	public class DatabasePermission
	{
		public string State { get; set; }
		public string StateDescription { get; set; }
		public string PermissionName { get; set; }

		public string ScriptCreate(string userName)
		{
			var sb = new StringBuilder();
			if (State != "W")	// W=Grant With Grant Option
				sb.Append(StateDescription);
			else
				sb.Append("GRANT");
			sb.Append($" {PermissionName} TO {userName}");
			if (State == "W")
				sb.Append(" WITH GRANT OPTION");

			return sb.ToString();
		}
	}

	public class SqlUser : INameable, IHasOwner, IScriptable {
		public List<string> DatabaseRoles = new List<string>();
		public string Owner { get; set; }
		public string Name { get; set; }
		public byte[] PasswordHash { get; set; }
		public List<ObjectPermission> ObjectPermissions { get; set; } = new List<ObjectPermission>();
		public List<DatabasePermission> DatabasePermissions { get; set; } = new List<DatabasePermission>();

		public SqlUser(string name, string owner) {
			Name = name;
			Owner = owner;
		}

		public void SortPermissions()
		{
			ObjectPermissions = ObjectPermissions
				.OrderBy(p => p.ObjectSchema)
				.ThenBy(p => p.ObjectName)
				.ThenBy(p => p.ColumnName)
				.ThenBy(p => p.State)
				.ThenBy(p => p.PermissionName)
				.ToList();

			DatabasePermissions = DatabasePermissions
				.OrderBy(p => p.State)
				.ThenBy(p => p.PermissionName)
				.ToList();
		}

		public string ScriptDrop() {
			return $"DROP USER [{Name}]";
			// NOTE: login is deliberately not dropped
		}

		public string ScriptCreate() {
			var sb = new StringBuilder();

			if (PasswordHash != null)
				sb.AppendLine($@"IF SUSER_ID('{Name}') IS NULL
				BEGIN CREATE LOGIN {Name} WITH PASSWORD = {"0x" + new SoapHexBinary(PasswordHash)} HASHED END");

			sb.AppendLine($"CREATE USER [{Name}] FOR LOGIN {Name} {(string.IsNullOrEmpty(Owner) ? string.Empty : "WITH DEFAULT_SCHEMA = ")}{Owner}");
			sb.AppendLine(string.Join("\r\n", DatabaseRoles.Select(r => $"/*ALTER ROLE {r} ADD MEMBER {Name}*/ exec sp_addrolemember '{r}', '{Name}'").ToArray()));
			sb.AppendLine();
			sb.AppendLine("--Object Level Permissions");
			sb.AppendLine(string.Join("\r\n", ObjectPermissions.Select(p => p.ScriptCreate(Name)).ToArray()));
			sb.AppendLine();
			sb.AppendLine("-- Database Level Permissions");
			sb.AppendLine(string.Join("\r\n", DatabasePermissions.Select(p => p.ScriptCreate(Name)).ToArray()));

			return sb.ToString();
		}
	}
}
