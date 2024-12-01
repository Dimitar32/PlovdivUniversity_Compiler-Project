using System.Text;

namespace scsc
{
	public class CharToken: LiteralToken
	{
		public char _value;
		
		public CharToken(int line, int column, char value): base(line, column) 
		{
			_value = value;
		}
		
		public override string ToString()
		{
			StringBuilder s = new StringBuilder();
			s.AppendFormat("_line {0}, _column {1}: {2} - {3}", line, column, _value, GetType());
			return s.ToString();
		}
	}
}
