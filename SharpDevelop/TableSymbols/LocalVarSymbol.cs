using System;
using System.Reflection;
using System.Text;

namespace scsc
{
	public class LocalVarSymbol: TableSymbol
	{
		public LocalVariableInfo _localVariableInfo;
		
		public LocalVarSymbol(IdentToken token, LocalVariableInfo localVariableInfo): base(token.line, token.column, token.value)
		{
			_localVariableInfo = localVariableInfo;
		}
		
		public override string ToString()
		{
			StringBuilder s = new StringBuilder();
			s.AppendFormat("_line {0}, _column {1}: {2} - {3} localvartype={4} localindex={5}", line, column, value, GetType(), _localVariableInfo.LocalType, _localVariableInfo.LocalIndex);
			return s.ToString();
		}
	}
}
