//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.13.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from MyGrammar.g4 by ANTLR 4.13.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using Antlr4.Runtime.Misc;
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="MyGrammarParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.1")]
[System.CLSCompliant(false)]
public interface IMyGrammarListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="MyGrammarParser.prog"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterProg([NotNull] MyGrammarParser.ProgContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MyGrammarParser.prog"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitProg([NotNull] MyGrammarParser.ProgContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MyGrammarParser.stat"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterStat([NotNull] MyGrammarParser.StatContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MyGrammarParser.stat"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitStat([NotNull] MyGrammarParser.StatContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="MyGrammarParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExpr([NotNull] MyGrammarParser.ExprContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="MyGrammarParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExpr([NotNull] MyGrammarParser.ExprContext context);
}
