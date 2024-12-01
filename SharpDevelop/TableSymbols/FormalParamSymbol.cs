using System;
using System.Reflection.Emit;
using System.Text;

namespace scsc
{
	public class FormalParamSymbol: TableSymbol
	{
		public Type _paramType;
		public ParameterBuilder _parameterInfo;
		
		public FormalParamSymbol(IdentToken token, Type paramType, ParameterBuilder parameterInfo): base(token.line, token.column, token.value)
		{
			_paramType = paramType;
			_parameterInfo = parameterInfo;
		}
		
		public override string ToString()
		{
			StringBuilder s = new StringBuilder();
			s.AppendFormat("_line {0}, _column {1}: {2} - {3} formalparamtype={4}", line, column, value, GetType(), _paramType);
			return s.ToString();
		}
	}
}
