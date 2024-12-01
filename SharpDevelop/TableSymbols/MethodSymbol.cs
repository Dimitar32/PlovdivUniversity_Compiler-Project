using System;
using System.Reflection;
using System.Text;

namespace scsc
{
	public class MethodSymbol: TableSymbol
	{
		public Type _returnType;
		public FormalParamSymbol[] _formalParams;
		public MethodInfo _methodInfo;
		
		public MethodSymbol(IdentToken token, Type returnType, FormalParamSymbol[] formalParams, MethodInfo methodInfo): base(token.line, token.column, token.value)
		{
			_returnType = returnType;
			_formalParams = formalParams;
			_methodInfo = methodInfo;
		}
		
//		public Type[] GetParamTypes()
//		{
//			Type[] paramTypes = new Type[_formalParams.Length];
//			for (int i=0; i<_formalParams.Length; i++) {
//				paramTypes[i] = _formalParams[i]._paramType;
//			}
//			return paramTypes;
//		}
		
		public override string ToString()
		{
			StringBuilder s = new StringBuilder();
			s.AppendFormat("_line {0}, _column {1}: {2} - {3} methodsignature={4} {5}(", line, column, value, GetType(), _returnType, value);
			foreach (FormalParamSymbol param in _formalParams) {
				s.AppendFormat("{0} {1}, ", param._paramType, param.value);
			}
			if (_formalParams.Length != 0) s.Remove(s.Length-2, 2);
			s.Append(")");
			return s.ToString();
		}
	}
}
