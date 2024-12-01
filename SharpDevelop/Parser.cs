using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;

namespace scsc
{
	public class Parser
    {
        public const string specialSymbol = "Expect a special symbol";

        private Scanner _scanner;
		private Emit _emit;
		private Table _symbolTable;
		private Token token;
		private Diagnostics _diag;

        private const string program = "Program";
        private const string system = "System";

        public Parser(Scanner scanner, Emit emit, Table symbolTable, Diagnostics diag)
		{
			_scanner = scanner;
			_emit = emit;
			_symbolTable = symbolTable;
			_diag = diag;
		}

        //public bool IsInfiniteLoopStatement()
        //{
        //    if (CheckKeyword("do")) 
        //    {
        //        _emit.AddInfiniteLoop(() =>
        //        {
        //            if (!IsStatement())
        //            {
        //                Error("Expected a statement after 'do'");
        //            }
        //        });

        //        if (!CheckKeyword("loop")) 
        //        {
        //            Error("Expected 'loop' to close the infinite loop");
        //            return false;
        //        }

        //        return true;
        //    }

        //    return false;
        //}

        public void AddPredefinedSymbols()
		{
			_symbolTable.AddToUniverse(new PrimitiveTypeSymbol(new IdentToken(-1,-1, "int"), typeof(System.Int32)));
			_symbolTable.AddToUniverse(new PrimitiveTypeSymbol(new IdentToken(-1, -1, "double"), typeof(System.Double)));
		}
		
		public bool Parse()
		{
			ReadNextToken();
			AddPredefinedSymbols();

			return IsProgram() && token is EOFToken;
		}
		
		public void ReadNextToken()
		{
			token = _scanner.Next();
		}
		
		public bool CheckKeyword(string keyword)
		{
			bool result = (token is KeywordToken) && ((KeywordToken)token)._value==keyword;
			if (result) 
				ReadNextToken();

			return result;
		}
		
		public bool CheckSpecialSymbol(string symbol)
		{
			bool result = (token is SpecialSymbolToken) && ((SpecialSymbolToken)token)._value==symbol;
			if (result) 
				ReadNextToken();

			return result;
        }
        
        public bool CheckEndOfFile()
        {
            return (token is EOFToken);
        }

        public bool CheckIdent()
		{
			bool result = (token is IdentToken);
			if (result) 
				ReadNextToken();

			return result;
		}
		
		public bool CheckNumber()
		{
			bool result = (token is NumberToken);
			if (result) 
				ReadNextToken();

			return result;
		}
		
		public bool CheckDouble()
		{
			bool result = (token is DoubleToken);
			if (result) 
				ReadNextToken();

			return result;
		}
		
		public bool CheckBoolean()
		{
			bool result = (token is BooleanToken);
			if (result) 
				ReadNextToken();
			
			return result;
		}
		
		public bool CheckChar()
		{
			bool result = (token is CharToken);
			if (result) 
				ReadNextToken();
			
			return result;
		}
		
		public bool CheckString()
		{
			bool result = (token is StringToken);
			if (result) 
				ReadNextToken();

			return result;
		}
		
		void SkipUntilSemiColon() 
		{
			Token Tok;
			
			do {
				Tok = _scanner.Next();
			} while (!((Tok is EOFToken) ||
				  	   (Tok is SpecialSymbolToken) && ((Tok as SpecialSymbolToken)._value == ";")));
		}
		
		public void Error(string message)
		{
			_diag.Error(token.line, token.column, message);
			SkipUntilSemiColon();
		}
		
		public void Error(string message, Token token)
		{
			_diag.Error(token.line, token.column, message);
			SkipUntilSemiColon();
		}
		
		public void Error(string message, Token token, params object[] par)
		{
			_diag.Error(token.line, token.column, string.Format(message, par));
			SkipUntilSemiColon();
		}
		
		public void Warning(string message)
		{
			_diag.Warning(token.line, token.column, message);
		}
		
		public void Warning(string message, Token token)
		{
			_diag.Warning(token.line, token.column, message);
		}
		
		public void Warning(string message, Token token, params object[] par)
		{
			_diag.Warning(token.line, token.column, string.Format(message, par));
		}
		
		public void Note(string message)
		{
			_diag.Note(token.line, token.column, message);
		}
		
		public void Note(string message, Token token)
		{
			_diag.Note(token.line, token.column, message);
		}
		
		public void Note(string message, Token token, params object[] par)
		{
			_diag.Note(token.line, token.column, string.Format(message, par));
		}
		
		public bool IsProgram()
		{
            AddIdentityToken();
            AddMethodInfo();

            while (IsStatement()) ;

            _symbolTable.EndScope();
            return _diag.GetErrorCount() == 0;
        }

        private void AddIdentityToken()
        {
            IdentToken identityToken = new IdentToken(1, 1, program);
            _symbolTable.AddUsingNamespace(system);
            _symbolTable.AddToUniverse(new PrimitiveTypeSymbol(identityToken, _emit.InitProgramClass(identityToken.value)));
        }

        public void AddMethodInfo()
        {
            IdentToken MainMethodName = new IdentToken(1, 1, "main");
            Type MainMathodType = typeof(Int32);

            List<FormalParamSymbol> formalParams = new List<FormalParamSymbol>();
            List<Type> formalParamTypes = new List<Type>();

            MethodSymbol mainMethodToken = _symbolTable.AddMethod(MainMethodName, MainMathodType, formalParams.ToArray(), null);

            _symbolTable.BeginScope();
            mainMethodToken._methodInfo = _emit.AddMethod(MainMethodName.value, MainMathodType, formalParamTypes.ToArray());
        }
        
        private void DeclareVariable()
        {
            if (token is IdentToken)
            {
                AddLocalVar();
            }
        }

        private void AddLocalVar()
        {
            IdentToken name = token as IdentToken;
            
			if (!_symbolTable.ExistCurrentScopeSymbol(name.value))
            {
                _symbolTable.AddLocalVar(name, _emit.AddLocalVar(name.value, typeof(System.Int32)));
            }
        }

        public bool IsStatement()
        {
            Type type = null;
            LocationInfo location;

            DeclareVariable();

            if (IsLocation(out location))
            {
                LocalVarSymbol localvar = location.id as LocalVarSymbol;

                if (localvar != null)
                {
                    if (CheckSpecialSymbol("="))
                    {
                        if (!IsExpression(null, out type))
                            Error("Expect an expression");

                        if (!CheckSpecialSymbol(";"))
                            Error(specialSymbol + "';'");

                        _emit.AddLocalVarAssigment(localvar._localVariableInfo);
                        return true;
                    }
                }
                CheckExpression(location, type);
            }
            else if (IsExpression(null, out type))
            {
                if (!CheckSpecialSymbol(";"))
                    Error(specialSymbol + "';'");
            }
            else
            {
                return false;
            }

            if (CheckEndOfFile())
                return false;

            return true;
        }

        private void CheckExpression(LocationInfo location, Type type)
        {
            if (!IsExpression(location, out type))
                Error("NOT FOUND IDENTIFIER", location.id);

            if (!CheckSpecialSymbol(";"))
                Error(specialSymbol + "' ; '");
        }
        
        public bool IsLocation(out LocationInfo location)
        {
            IdentToken id = token as IdentToken;

            if (!CheckIdent())
            {
                location = null;
                return false;
            }

            location = new LocationInfo();
            location.id = _symbolTable.GetSymbol(id.value);

            if (location.id == null) Error("Identificator has not been declared!!! {0}", id, id.value);

            return true;
        }
        
        public bool IsExpression(LocationInfo location, out Type type)
        {
            SpecialSymbolToken opToken;

            if (!IsBitwiseAndExpression(location, out type))
                return false;

            opToken = token as SpecialSymbolToken;

            while (CheckSpecialSymbol("|"))
            {
                if (!IsBitwiseAndExpression(null, out type))
                    Error("Expected BitwiseAndExpression");

                _emit.AddAdditiveOp(opToken._value);

                opToken = token as SpecialSymbolToken;
            }

            return true;
        }
        
        public bool IsBitwiseAndExpression(LocationInfo location, out Type type)
        {
            SpecialSymbolToken opToken;
            if (!IsAdditiveExpr(location, out type))
                return false;

            opToken = token as SpecialSymbolToken;
            while (CheckSpecialSymbol("&"))
            {
                if (!IsAdditiveExpr(null, out type)) Error("Expect AdditiveExpression");

                _emit.AddMultiplicativeOp(opToken._value);
                opToken = token as SpecialSymbolToken;
            }

            return true;
        }

        public bool IsAdditiveExpr(LocationInfo location, out Type type)
        {
            SpecialSymbolToken opToken;

            if (!IsMultiplicativeExpr(location, out type)) return false;

            opToken = token as SpecialSymbolToken;

            while (CheckSpecialSymbol("+") || CheckSpecialSymbol("-"))
            {

                if (!IsMultiplicativeExpr(null, out type)) Error("Expected MultiplicativeExpression");

                _emit.AddAdditiveOp(opToken._value);
                opToken = token as SpecialSymbolToken;
            }
            return true;
        }

        public bool IsMultiplicativeExpr(LocationInfo location, out Type type)
        {
            SpecialSymbolToken opToken;

            if (!isDoubleNumber(location, out type)) return false;

            opToken = token as SpecialSymbolToken;
            while (CheckSpecialSymbol("*") || CheckSpecialSymbol("/") || CheckSpecialSymbol("%"))
            {
                if (!IsPrimaryExpression(null, out type))
                    Error("Expect PrimaryExpression");

                if (opToken != null)
                {
                    _emit.AddMultiplicativeOp(opToken._value);

                    opToken = token as SpecialSymbolToken;
                }
                else
                    Error("MultiplicativeExpression -> opToken == null");
            }
            return true;
        }

        public bool isDoubleNumber(LocationInfo location, out Type type)
        {
            DoubleToken opToken;

            if (!IsPrimaryExpression(location, out type)) return false;

            opToken = token as DoubleToken;
            while (CheckSpecialSymbol("."))
            {
                if (!IsPrimaryExpression(null, out type))
                    Error("Expect decimal");

                if (opToken != null)
                {
                    _emit.AddGetDouble(opToken._value);

                    opToken = token as DoubleToken;
                }
                else
                    Error("MultiplicativeExpression -> opToken == null");
            }
            return true;
        }
        
        public enum IncDecOps { None, PreInc, PreDec, PostInc, PostDec }
        
        public bool IsPrimaryExpression(LocationInfo location, out Type type)
        {
            SpecialSymbolToken opToken;
            Token Numbliteral = token;
            IncDecOps incDecOp = IncDecOps.None;

            if (location != null)
            {
                opToken = null;
            }
            else
            {
                opToken = token as SpecialSymbolToken;
                if (CheckSpecialSymbol("++"))
                    incDecOp = IncDecOps.PreInc;

                else if (CheckSpecialSymbol("--"))
                    incDecOp = IncDecOps.PreDec;

                if (!IsLocation(out location) && incDecOp != IncDecOps.None)
                    Error("Expect a variable, argument or a field");
            }


            if (incDecOp == IncDecOps.None)
            {
                opToken = token as SpecialSymbolToken;
                if (CheckSpecialSymbol("++"))
                    incDecOp = IncDecOps.PostInc;

                else if (CheckSpecialSymbol("--"))
                    incDecOp = IncDecOps.PostDec;
            }

            if (location != null)
            {
                LocalVarSymbol localvar = location.id as LocalVarSymbol;
                if (localvar != null)
                {
                    type = localvar._localVariableInfo.LocalType;

                    _emit.AddGetLocalVar(localvar._localVariableInfo);

                    // 2. Емисия за инкрементиране/декрементиране
                    if (incDecOp == IncDecOps.PreInc || incDecOp == IncDecOps.PreDec ||
                        incDecOp == IncDecOps.PostInc || incDecOp == IncDecOps.PostDec)
                    {
                        _emit.AddIncLocalVar(localvar._localVariableInfo, incDecOp);
                    }

                    if (incDecOp == IncDecOps.PostInc || incDecOp == IncDecOps.PostDec)
                    {
                        _emit.AddPop(); 
                    }

                    return true;
                }
            }

            if (CheckSpecialSymbol("~"))
            {
                if (!IsPrimaryExpression(null, out type))
                    Error("Expect PrimaryExpression");

                _emit.AddUnaryOp(opToken._value);
            }

            if (CheckKeyword("do"))
            {
                if (!IsStatement())
                    Error("Expect a statement");

                if (CheckKeyword("loop"))

                _emit.AddInfiniteLoop();

                type = typeof(int);
                return true;
            }

            if (CheckKeyword("printf"))
            {
                if (!CheckSpecialSymbol("("))
                    Error(specialSymbol + "(");

                if (!IsExpression(null, out type))
                    Error("Expect a expression");

                if (!CheckSpecialSymbol(")"))
                    Error(specialSymbol + ")");

                MethodInfo bestMethodInfo = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(int) });

                if (bestMethodInfo != null)
                {
                    _emit.AddMethodCall(bestMethodInfo);
                }
                else
                    Error("ScanF cannot be Used!");

                type = bestMethodInfo.ReturnType;

                return true;
            }

            if (CheckKeyword("printd"))
            {
                if (!CheckSpecialSymbol("("))
                    Error(specialSymbol + "(");

                if (!IsExpression(null, out type))
                    Error("Expect a expression");

                if (!CheckSpecialSymbol(")"))
                    Error(specialSymbol + ")");

                MethodInfo bestMethodInfo = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(double) });

                if (bestMethodInfo != null)
                {
                    _emit.AddMethodCall(bestMethodInfo);
                }
                else
                    Error("ScanD cannot be Used!");

                type = bestMethodInfo.ReturnType;
                return true;
            }

            if (CheckDouble())
            {
                type = typeof(System.Double);

                _emit.AddGetDouble(((DoubleToken)Numbliteral)._value);
                return true;
            }

            if (CheckNumber())
            {
                type = typeof(System.Int32);

                _emit.AddGetNumber(((NumberToken)Numbliteral)._value);
                return true;
            }

            if (CheckKeyword("scanf"))
            {
                if (!CheckSpecialSymbol("("))
                    Error(specialSymbol + "(");

                if (!CheckSpecialSymbol(")"))
                    Error(specialSymbol + ")");

                MethodInfo bestMethodInfo = typeof(Console).GetMethod("ReadLine");
                MethodInfo convertInt32M = typeof(Convert).GetMethod("ToInt32", new Type[] { typeof(string) });

                if (bestMethodInfo != null)
                {
                    _emit.AddMethodCall(bestMethodInfo);
                    _emit.AddMethodCall(convertInt32M);
                }
                else
                    Error("There is no suitable combination of parameter types for the method scanf");

                type = bestMethodInfo.ReturnType;

                return true;
            }

            if (CheckKeyword("scand"))
            {
                if (!CheckSpecialSymbol("("))
                    Error(specialSymbol + "(");

                if (!CheckSpecialSymbol(")"))
                    Error(specialSymbol + ")");

                MethodInfo bestMethodInfo = typeof(Console).GetMethod("ReadLine");
                MethodInfo convertDouble = typeof(Convert).GetMethod("ToDouble", new Type[] { typeof(string) });

                if (bestMethodInfo != null)
                {
                    _emit.AddMethodCall(bestMethodInfo);
                    _emit.AddMethodCall(convertDouble);
                }
                else
                    Error("There is no suitable combination of parameter types for the method scanf");

                type = bestMethodInfo.ReturnType;
                return true;
            }

            //Check Symbol
            if (CheckSpecialSymbol("("))
            {
                if (!IsExpression(location, out type))
                    Error("Exepect an expression");

                if (!CheckSpecialSymbol(")"))
                    Error(specialSymbol + ")");

                return true;
            }

            type = null;
            return true;
        }

        public class LocationInfo
        {
            public TableSymbol id;
            public bool isArray;
        }

	}
}
