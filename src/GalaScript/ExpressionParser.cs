using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GalaScript.Evaluators;
using GalaScript.Interfaces;
using Sprache;

namespace GalaScript
{
    public class ExpressionParser : IParser
    {
        private readonly ScriptEngine _engine;

        private string _path;

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

        private static readonly Parser<IEvaluator> QuotedString =
            from open in DoubleQuote
            from text in QuotedPair.Or(QdText).Many().Text()
            from close in DoubleQuote
            select new StringConstantEvaluator(text);

        private static readonly Parser<string> Comment =
            from op in Parse.String("#").Or(Parse.String("//"))
            from space in Space.Optional()
            from comment in Parse.CharExcept('\n').Many().Text()
            select comment;

        private static readonly Parser<IEvaluator> Decimal =
            from op in Parse.Char('-').Optional()
            from n in Parse.Number.Optional()
            from dot in Parse.Char('.')
            from f in Parse.Number
            from type in Parse.Char('m').Optional()
            select new DecimalConstantEvaluator(decimal.Parse($"{n.GetOrElse("0")}.{f}") * (op.IsDefined ? -1 : 1));

        private static readonly Parser<IEvaluator> DecimalWithType =
            from op in Parse.Char('-').Optional()
            from num in Parse.Number
            from type in Parse.Char('m')
            select new DecimalConstantEvaluator(decimal.Parse(num) * (op.IsDefined ? -1 : 1));

        private static readonly Parser<IEvaluator> Integer =
            from op in Parse.Char('-').Optional()
            from num in Parse.Number
            from type in Parse.Char('L').Optional()
            select new IntegerConstantEvaluator(long.Parse(num) * (op.IsDefined? -1 : 1));

        private static readonly Parser<IEvaluator> Number = DecimalWithType.Or(Decimal).Or(Integer);

        private static readonly Parser<IEvaluator> Constant =
            from k in Token
            select new ConstantEvaluator(k);

        private static readonly Parser<IEvaluator> Label =
            from op in Parse.Char('*')
            from space in Space.Optional()
            from label in Token
            select new LabelEvaluator(label);

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

        private readonly Parser<IEvaluator> Text;

        private readonly Parser<IEvaluator> Macro;

        private readonly Parser<IEvaluator> Import;

        private readonly Parser<IEvaluator> NamedParameter;

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

            NamedParameter =
                from name in Token
                from leading in Space.Optional()
                from op in Parse.Char('=')
                from trailing in Space.Optional()
                from value in Function.Or(Number).Or(QuotedString).Or(Ret).Or(Identifier).Or(Constant)
                select new NamedParameterEvaluator(name, value);

            Function =
                from lparen in Parse.Char('[')
                from _ in Space.Optional()
                from name in Token
                from space in Space.Optional()
                from expr in Parse.Ref(() => NamedParameter.Or(Label).Or(Function).Or(Number).Or(QuotedString).Or(Ret).Or(Identifier).Or(Constant)).DelimitedBy(Space).Optional()
                from trailing in Space.Optional()
                from rparen in Parse.Char(']')
                select new FunctionEvaluator(_engine, name, expr.GetOrDefault()?.ToArray() ?? Array.Empty<IEvaluator>());

            Alias =
                from value in Function.Or(Number).Or(QuotedString).Or(Ret).Or(Identifier).Or(Constant)
                from leading in Space.Optional()
                from op in Parse.Char(':')
                from trailing in Space.Optional()
                from name in Variable
                select new AliasEvaluator(_engine, name, value);

            Text =
                from op in Parse.Char('-').Or(Parse.Char('+'))
                from space in Space.Optional()
                from text in Parse.CharExcept('\n').AtLeastOnce().Text().Optional()
                select new TextEvaluator(_engine, text.GetOrDefault(), op);

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
                select new ScriptEvaluator(_engine, File.ReadAllText(Path.Combine(_path, file)));

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
            _path = Path.GetDirectoryName(Environment.CurrentDirectory);

            return new ScriptEvaluator(_engine, str);
        }

        public IScriptEvaluator LoadFile(string file, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.Default;
            }

            _path = Path.GetDirectoryName(Path.GetFullPath(file));

            var str = File.ReadAllText(file, encoding);

            return new ScriptEvaluator(_engine, str);
        }
    }
}
