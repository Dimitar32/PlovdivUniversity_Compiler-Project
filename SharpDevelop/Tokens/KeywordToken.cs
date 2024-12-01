using System.Text;

namespace scsc
{
	public class KeywordToken: Token
	{
		public string _value;
		
		public KeywordToken(int line, int column, string value): base(line, column) 
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
