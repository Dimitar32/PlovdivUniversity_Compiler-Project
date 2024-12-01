using System;
using System.Reflection;
using System.Text;

namespace scsc
{
	public class FieldSymbol: TableSymbol 
	{
		public FieldInfo _fieldInfo;
		
		public FieldSymbol(IdentToken token, FieldInfo fieldInfo): base(token.line, token.column, token.value)
		{
			_fieldInfo = fieldInfo;
		}
		
		public override string ToString()
		{
			StringBuilder s = new StringBuilder();
			s.AppendFormat("_line {0}, _column {1}: {2} - {3} fieldtype={4}", line, column, value, GetType(), _fieldInfo.FieldType);
			return s.ToString();
		}
	}
}
