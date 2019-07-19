using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GalaScript.Evaluators;
using GalaScript.Abstract;
using Sprache;

namespace GalaScript
{
    public class ExpressionParser : IParser
    {
        private readonly ScriptEngine _engine;

        private string path;

        #region grammars

        private static readonly Parser<char> Backslash = Parse.Char('\\');
        private static readonly Parser<char> DoubleQuote = Parse.Char('"');

        private static readonly Parser<string> Token =
            Parse.Identifier(Parse.Letter, Parse.LetterOrDigit.Or(Parse.Char('_')));

        private static readonly Parser<string> Variable =
            Parse.Identifier(Parse.Char('$'), Parse.LetterOrDigit.Or(Parse.Char('_')));

        private static readonly Parser<string> Space = Parse.WhiteSpace.Except(Parse.LineEnd).Many().Text();

        private static readonly Parser<char> QdText = Parse.AnyChar.Except(DoubleQuote);

        private static readonly Parser<char> QuotedPair =
            from _ in Backslash
            from c in Parse.AnyChar
            select c;

        private static readonly Parser<StringConstantEvaluator> QuotedString =
            from open in DoubleQuote
            from text in QuotedPair.Or(QdText).Many().Text()
            from close in DoubleQuote
            select new StringConstantEvaluator(text);

        private static readonly Parser<string> Comment =
            from op in Parse.Char('#')
            from space in Space.Optional()
            from comment in Parse.CharExcept('\n').Many().Text()
            select comment;

        private static readonly Parser<DecimalConstantEvaluator> Number =
            from op in Parse.Char('-').Optional()
            from num in Parse.Decimal
            select new DecimalConstantEvaluator(decimal.Parse(num) * (op.IsDefined ? -1 : 1));

        private static readonly Parser<ConstantEvaluator> Constant =
            from k in Token
            select new ConstantEvaluator(k);

        private static readonly Parser<IEvaluator> Label =
            from op in Parse.Char('*')
            from space in Space.Optional()
            from label in Token
            select new LabelEvaluator(label);

        private static readonly Parser<IEvaluator> Text =
            from op in Parse.Char('-').Or(Parse.Char('+'))
            from space in Space.Optional()
            from text in Parse.CharExcept('\n').AtLeastOnce().Text().Optional()
            select new TextEvaluator(text.GetOrDefault(), op == '-');

        private static readonly Parser<IEnumerable<string>> MacroParameters =
            from lparen in Parse.Char('[')
            from parameters in Variable.DelimitedBy(Space)
            from rparen in Parse.Char(']')
            select parameters;

        // ReSharper disable InconsistentNaming
        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly Parser<AliasEvaluator> Identifier;

        private readonly Parser<AliasEvaluator> Ret;

        private readonly Parser<IEvaluator> Function;

        private readonly Parser<IEvaluator> Alias;

        private readonly Parser<IEvaluator> Macro;

        private readonly Parser<IEvaluator> Import;

        private Parser<IEvaluator> Evaluator;

        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable
        // ReSharper restore InconsistentNaming

        #endregion

        public ExpressionParser(ScriptEngine engine)
        {
            _engine = engine;

            Identifier =
                from name in Variable
                select new AliasEvaluator(_engine, name);

            Ret =
                from name in Parse.String("ret").Text()
                select new AliasEvaluator(_engine, name);

            Function =
                from lparen in Parse.Char('[')
                from _ in Space.Optional()
                from name in Token
                from space in Space.Optional()
                from expr in Parse.Ref(() => Label.Or(Number).Or(QuotedString).Or(Ret).Or(Identifier).Or(Constant))
                    .DelimitedBy(Space).Optional()
                from trailing in Space.Optional()
                from rparen in Parse.Char(']')
                select new FunctionEvaluator(_engine, name, expr.GetOrDefault()?.ToArray() ?? new IEvaluator[0]);

            Alias =
                from func in Function
                from leading in Space.Optional()
                from op in Parse.Char(':')
                from trailing in Space.Optional()
                from name in Variable
                select new AliasEvaluator(_engine, name, func);

            Macro =
                from op in Parse.Char('!')
                from _ in Space.Optional()
                from name in Token
                from space in Space.Optional()
                from parameters in MacroParameters.Optional()
                from eol in Parse.LineEnd
                from str in Parse.CharExcept('!').AtLeastOnce().Text()
                from ending in Parse.Char('!')
                select new MacroEvaluator(_engine, name, parameters.GetOrDefault()?.ToArray(), str);

            Import =
                from op in Parse.Char('%')
                from space in Space.Optional()
                from file in Parse.AnyChar.Except(Parse.LineTerminator).Many().Text()
                select new ScriptEvaluator(_engine, File.ReadAllText(Path.Combine(path, file)));

            Evaluator = Import.Or(Macro).Or(Label).Or(Text).Or(Alias).Or(Function);
        }

        public void RegisterEvaluator(Parser<IEvaluator> parser)
        {
            Evaluator = Evaluator.Or(parser);
        }

        public IEnumerable<IEvaluator> Prepare(string str)
        {
            var parser =
                from leading in Space.Optional()
                from evaluator in Evaluator.Optional()
                from space in Space.Optional()
                from comment in Comment.Optional()
                from trailing in Space.Optional()
                from eol in Parse.LineTerminator
                select evaluator.GetOrDefault();

            return parser.Many().End().Parse(str);
        }

        public IScriptEvaluator LoadString(string str)
        {
            path = Path.GetDirectoryName(Environment.CurrentDirectory);

            return new ScriptEvaluator(_engine, str);
        }

        public IScriptEvaluator LoadFile(string file, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.Default;
            }

            path = Path.GetDirectoryName(Path.GetFullPath(file));

            var str = File.ReadAllText(file, encoding);

            return new ScriptEvaluator(_engine, str);
        }
    }
}
