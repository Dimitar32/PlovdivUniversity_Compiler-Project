using System;
using System.Text;

namespace scsc
{
	public abstract class TypeSymbol: TableSymbol
	{
		public Type _type;
		
		public TypeSymbol(IdentToken token, Type type): base(token.line, token.column, token.value)
		{
			_type = type;
		}
		
		public override string ToString()
		{
			StringBuilder s = new StringBuilder();
			s.AppendFormat("_line {0}, _column {1}: {2} - {3} _type={4}", line, column, value, GetType(), _type.FullName);
			return s.ToString();
		}
	}
}
