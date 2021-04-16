using System;
using AGRO_GRAMM;



using System;



public class Parser {
	public const int _EOF = 0;
	public const int _id = 1;
	public const int _cte_I = 2;
	public const int _cte_F = 3;
	public const int _ctr_Str = 4;
	public const int _cbl = 5;
	public const int _cbr = 6;
	public const int _bl = 7;
	public const int _br = 8;
	public const int _pl = 9;
	public const int _pr = 10;
	public const int _comma = 11;
	public const int _semicolon = 12;
	public const int _add = 13;
	public const int _sub = 14;
	public const int _mul = 15;
	public const int _div = 16;
	public const int _equal = 17;
	public const int _dot = 18;
	public const int _sadd = 19;
	public const int _ssub = 20;
	public const int _sdiv = 21;
	public const int _smul = 22;
	public const int _increment = 23;
	public const int _decrement = 24;
	public const int _colon = 25;
	public const int _less = 26;
	public const int _greater = 27;
	public const int _lesseq = 28;
	public const int _greatereq = 29;
	public const int _equaleq = 30;
	public const int _different = 31;
	public const int maxT = 36;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

const int // types
	  undef = 0, t_int = 1, t_float = 2, t_char = 3, t_id = 4;

const int // object kinds
	  var = 0, proc = 1;

SymbolTable   sTable;

/*--------------------------------------------------------------------------*/    

bool IsFunctionCall(){
    scanner.ResetPeek();
    Token x = la; 
    while (x.kind == _id ) 
        x = scanner.Peek();
    return x.kind == _pl;
}

bool IsMethodCall() { 
    scanner.ResetPeek();
    Token x = la; 
    while (x.kind == _id || x.kind == _dot) 
        x = scanner.Peek();
    return x.kind == _pl;
} 

bool IsTypeFunction() {
    scanner.ResetPeek();
    Token next = scanner.Peek();
    next = scanner.Peek();
    return next.kind == _pl;
}

bool IsDecVars(){
    scanner.ResetPeek();
    Token x = scanner.Peek();
    while (x.kind == _id || x.kind == _comma) 
        x = scanner.Peek();
    return x.kind == _semicolon;
}



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void PROGRAM() {
		Console.WriteLine("Entering PROGRAM"); 
		sTable = new SymbolTable();
		
		while (la.kind == 32 || la.kind == 33 || la.kind == 34) {
			DECLARATION();
		}
		MAIN();
	}

	void DECLARATION() {
		DEC_VARS();
	}

	void MAIN() {
		sTable = sTable.newChildSymbolTable(); 
		Expect(35);
		Expect(5);
		DEC_VARS();
		while (la.kind == 32 || la.kind == 33 || la.kind == 34) {
			DEC_VARS();
		}
		Expect(6);
		Console.WriteLine(sTable.getSymbol("aber")[0]);
		sTable = sTable.parentSymbolTable; 
	}

	void DEC_VARS() {
		string name; int type; 
		SIMPLE_TYPE(out type );
		IDENT(out name );
		sTable.putSymbol(name, type, var); 
		if (la.kind == 7) {
			Get();
			Expect(2);
			Expect(8);
			if (la.kind == 7) {
				Get();
				Expect(2);
				Expect(8);
			}
		}
		while (la.kind == 11) {
			Get();
			IDENT(out name );
			sTable.putSymbol(name, type, var); 
			if (la.kind == 7) {
				Get();
				Expect(2);
				Expect(8);
				if (la.kind == 7) {
					Get();
					Expect(2);
					Expect(8);
				}
			}
		}
		Expect(12);
	}

	void SIMPLE_TYPE(out int type ) {
		type = undef; 
		if (la.kind == 32) {
			Get();
			type = t_int; 
		} else if (la.kind == 33) {
			Get();
			type = t_float; 
		} else if (la.kind == 34) {
			Get();
			type = t_char; 
		} else SynErr(37);
	}

	void IDENT(out string name ) {
		Expect(1);
		name = t.val; 
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		PROGRAM();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "id expected"; break;
			case 2: s = "cte_I expected"; break;
			case 3: s = "cte_F expected"; break;
			case 4: s = "ctr_Str expected"; break;
			case 5: s = "cbl expected"; break;
			case 6: s = "cbr expected"; break;
			case 7: s = "bl expected"; break;
			case 8: s = "br expected"; break;
			case 9: s = "pl expected"; break;
			case 10: s = "pr expected"; break;
			case 11: s = "comma expected"; break;
			case 12: s = "semicolon expected"; break;
			case 13: s = "add expected"; break;
			case 14: s = "sub expected"; break;
			case 15: s = "mul expected"; break;
			case 16: s = "div expected"; break;
			case 17: s = "equal expected"; break;
			case 18: s = "dot expected"; break;
			case 19: s = "sadd expected"; break;
			case 20: s = "ssub expected"; break;
			case 21: s = "sdiv expected"; break;
			case 22: s = "smul expected"; break;
			case 23: s = "increment expected"; break;
			case 24: s = "decrement expected"; break;
			case 25: s = "colon expected"; break;
			case 26: s = "less expected"; break;
			case 27: s = "greater expected"; break;
			case 28: s = "lesseq expected"; break;
			case 29: s = "greatereq expected"; break;
			case 30: s = "equaleq expected"; break;
			case 31: s = "different expected"; break;
			case 32: s = "\"int\" expected"; break;
			case 33: s = "\"float\" expected"; break;
			case 34: s = "\"char\" expected"; break;
			case 35: s = "\"main\" expected"; break;
			case 36: s = "??? expected"; break;
			case 37: s = "invalid SIMPLE_TYPE"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
