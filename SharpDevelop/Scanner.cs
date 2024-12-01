using System;
using System.IO;
using System.Text;

namespace scsc
{
	public class Scanner
	{
		const char EOF = '\u001a';
		const char CR = '\r';
		const char LF = '\n';
		//const char Escape = '\\';

		static readonly string keywords = "printf scanf do"; //" object void null using if else while return break continue class as is ";

        static readonly string specialSymbols1 = "{}(),;~";
        static readonly string specialSymbols2 = "*%/!&|+-<=>";
        static readonly string specialSymbols2Pairs = "+= -= *= /= %= != && || ++ -- <= == >=";

        private TextReader _reader;
		private char ch;
		private int _line, _column;
		private bool skipComments = true;	

		public bool SkipComments {
			get { return skipComments; }
			set { skipComments = value; }
		}
		
		public Scanner(TextReader reader)
		{
			_reader = reader;
			_line = 1;
			_column = 0;

			ReadNextChar();
		}
		
		public void ReadNextChar()
		{
			int ch1 = _reader.Read();
			_column++;
			ch = (ch1<0) ? EOF : (char)ch1;
			if (ch==CR) {
				_line++;
				_column = 0;
			} else if (ch==LF) {
				_column = 0;
			}
		}
		
		//public char UnEscape(char c)
		//{
		//	switch (c) {
		//		case 't': return '\t';
		//		case 'n': return '\n';
		//		case 'r': return '\r';
		//		case 'f': return '\f';
		//		case '\'': return '\'';
		//		case '"': return '\"';
		//		case '0': return '\0';
		//		case Escape: return Escape;
		//		default:  return c;
		//	}
		//}
		
		public Token Next()
		{
			int startColumn;
			int startLine;

			while (true) {
				startColumn = _column;
				startLine = _line;
				
				if (ch>='a' && ch<='z' || ch>='A' && ch<='Z' || ch=='_' || ch=='.') {
					StringBuilder s = new StringBuilder();
					
					while (ch>='a' && ch<='z' || ch>='A' && ch<='Z' || ch=='_' || ch=='.' || ch>='0' && ch<='9') {
						s.Append(ch);
						ReadNextChar();
					}

					string id = s.ToString();
  					
					if (id.Equals("false") || id.Equals("true") ) 
					{
						return new BooleanToken(startLine, startColumn, id.Equals("true"));
					} 
					else if (keywords.Contains(id)) 
					{
						return new KeywordToken(startLine, startColumn, id);
					}

					return new IdentToken(startLine, startColumn, id);
				} 
				else if (ch>='0' && ch<='9') 
				{
					StringBuilder s = new StringBuilder();
					
					while (ch>='0' && ch<='9') 
					{
						s.Append(ch);
						ReadNextChar();
					}

					if (ch=='.') 
					{
						s.Append(ch);
						ReadNextChar();
						
						while (ch>='0' && ch<='9') 
						{
							s.Append(ch);
							ReadNextChar();
						}

						return new DoubleToken(startLine, startColumn, Convert.ToDouble(s.ToString(), System.Globalization.NumberFormatInfo.InvariantInfo));
					}

					return new NumberToken(startLine, startColumn, Convert.ToInt64(s.ToString()));
				} 
				else if (ch=='\'') 
				{
					ReadNextChar();
					char ch1 = ch;
					ReadNextChar();
					if (ch=='\'') ReadNextChar();
					return new CharToken(startLine, startColumn, ch1);
				} 
				else if (ch=='"') 
				{
					StringBuilder s = new StringBuilder();
					ReadNextChar();
					
					while (ch!='"' && ch!=EOF) 
					{
						char ch1 = ch;
						s.Append(ch1);
						ReadNextChar();
					}

					ReadNextChar();

					return new StringToken(startLine, startColumn, s.ToString());
				} 
				else if (specialSymbols1.Contains(ch.ToString())) 
				{
					char ch1 = ch;
					ReadNextChar();

					return new SpecialSymbolToken(startLine, startColumn, ch1.ToString());
				} 
				else if (specialSymbols2.Contains(ch.ToString())) 
				{
					char ch1 = ch;
					ReadNextChar();
					char ch2 = ch;

					if (specialSymbols2Pairs.Contains(" " + ch1 + ch2 + " ")) 
					{
						ReadNextChar();
						return new SpecialSymbolToken(startLine, startColumn, ch1.ToString()+ch2);
					}

					return new SpecialSymbolToken(startLine, startColumn, ch1.ToString());
				} 
				else if (ch==' ' || ch=='\t' || ch==CR || ch==LF) 
				{
					ReadNextChar();
					continue;
				} 
				else if (ch=='/') 
				{
					char ch1 = ch;
					ReadNextChar();

					if (ch=='/') 
					{
						if (skipComments) 
						{
							while (ch!=CR && ch!=LF && ch!=EOF) 
							{
								ReadNextChar();
							}
							
							ReadNextChar();
						} 
						else 
						{
							StringBuilder s = new StringBuilder();

							while (ch!=CR && ch!=LF && ch!=EOF) 
							{
								ReadNextChar();
								s.Append(ch);
							}

							ReadNextChar();

							return new CommentToken(startLine, startColumn, s.ToString(), true);
						}
					} 
					else if (ch=='*') 
					{
						if (skipComments) {
							ReadNextChar();
							do {
								while (ch!='*' && ch!=EOF) {
									ReadNextChar();
								}
								ReadNextChar();
							} while (ch!='/' && ch!=EOF);
							ReadNextChar();
						} else {
							StringBuilder s = new StringBuilder();
							ReadNextChar();
							do {
								while (ch!='*' && ch!=EOF) {
									s.Append(ch);
									ReadNextChar();
								}
								ReadNextChar();
							} while (ch!='/' && ch!=EOF);
							ReadNextChar();
							return new CommentToken(startLine, startColumn, s.ToString(), false);
						}
					} 
					else
					{
						return new SpecialSymbolToken(startLine, startColumn, ch1.ToString());
					}	
					continue;
				} 
				else if (ch==EOF) 
				{
					return new EOFToken(startLine, startColumn);
				} 
				else 
				{
					string s = ch.ToString();
					ReadNextChar();
					return new OtherToken(startLine, startColumn, s);
				}
			}
		}
	}
}
