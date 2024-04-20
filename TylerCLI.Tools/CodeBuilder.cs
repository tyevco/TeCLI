using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TylerCLI
{
    public class CodeBuilder
    {
        private StringBuilder Builder { get; }

        private int IndentLevel { get; set; } = 0;

        private const char IndentChar = ' ';

        private HashSet<string> Usings { get; } = new HashSet<string>();

        public CodeBuilder(params string[] usings)
        {
            Builder = new StringBuilder();

            if (usings.Length > 0)
            {
                foreach (var ns in usings)
                {
                    var value = ns.Trim();
                    value = value.StartsWith("using ") ? value.Substring(6) : value;
                    value = value.EndsWith(";") ? value.Substring(0, value.Length - 1) : value;

                    if (!string.IsNullOrWhiteSpace(value))
                        Usings.Add(value);
                }
            }
        }

        public void AddUsing(string ns)
        {
            if (!string.IsNullOrWhiteSpace(ns))
                Usings.Add(ns);
        }

        public IDisposable AddBlock(string line, int indentAmount = 4, string openChar = "{", string closeChar = "}")
        {
            AppendLine(line);
            return AddBlankScope(indentAmount, openChar, closeChar);
        }

        public TryCatchBuilder AddTry(int indentAmount = 4)
        {
            AppendLine("try");
            return new TryCatchBuilder(
                () =>
                {
                    AppendLine("{");
                    IndentLevel += indentAmount;
                },
                expr =>
                {
                    AppendLine(expr);
                },
                () =>
                {
                    IndentLevel -= indentAmount;
                    if (IndentLevel < 0)
                        IndentLevel = 0;

                    AppendLine("}");
                });
        }

        public IfElseBuilder AddIf(string expression, int indentAmount = 4)
        {
            AppendLine($"if ({expression})");
            return new IfElseBuilder(
                () =>
                {
                    AppendLine("{");
                    IndentLevel += indentAmount;
                },
                expr =>
                {
                    AppendLine(expr);
                },
                () =>
                {
                    IndentLevel -= indentAmount;
                    if (IndentLevel < 0)
                        IndentLevel = 0;

                    AppendLine("}");
                });
        }

        public IDisposable AddBlankScope(int indentAmount = 4, string openChar = "{", string closeChar = "}")
        {
            AppendLine(openChar);

            IndentLevel += indentAmount;

            return new OnDispose(() =>
            {
                IndentLevel -= indentAmount;
                if (IndentLevel < 0)
                    IndentLevel = 0;

                AppendLine(closeChar);
            });
        }

        public IDisposable Indent(int indentAmount = 4)
        {
            IndentLevel += indentAmount;
            return new OnDispose(() =>
            {
                IndentLevel -= indentAmount;
                if (IndentLevel < 0)
                    IndentLevel = 0;
            });
        }

        public void AppendLine(string line)
        {
            Builder.AppendFormat("{0}{1}\n", new string(IndentChar, IndentLevel >= 0 ? IndentLevel : 0), line);
        }

        public void AddBlankLine()
        {
            Builder.AppendLine(string.Empty);
        }

        public override string ToString()
        {
            string usingBlock = string.Empty;

            if (Usings.Count(ns => !string.IsNullOrWhiteSpace(ns)) > 0)
            {
                usingBlock = string.Join("\n", Usings.Where(ns => !string.IsNullOrWhiteSpace(ns)).Select(u => string.Format("using {0};", u)));

                return $"{usingBlock}\n\n{Builder}";
            }
            else
            {
                return Builder.ToString();
            }
        }

        public class OnDispose : IDisposable
        {
            public OnDispose(Action onDispose)
            {
                EndBlock = onDispose;
            }

            protected Action EndBlock { get; }

            public void Dispose()
            {
                EndBlock?.Invoke();
            }
        }

        public abstract class BlockBuilder : OnDispose
        {
            public BlockBuilder(Action addBlock, Action<string> appendLine, Action onDispose) : base(onDispose)
            {
                AddBlock = addBlock;
                AppendLine = appendLine;

                addBlock?.Invoke();
            }

            protected Action AddBlock { get; }
            protected Action<string> AppendLine { get; }

            protected void AddBlockExpression(string expression)
            {
                EndBlock?.Invoke();
                AppendLine?.Invoke(expression);
                AddBlock?.Invoke();
            }
        }


        public class TryCatchBuilder : BlockBuilder
        {
            public TryCatchBuilder(Action addBlock, Action<string> appendLine, Action onDispose) : base(addBlock, appendLine, onDispose)
            {
            }

            public TryCatchBuilder Catch(string expression = "")
            {
                EndBlock?.Invoke();
                AppendLine?.Invoke($"catch{(string.IsNullOrWhiteSpace(expression) ? string.Empty : $" {expression}")}");
                AddBlock?.Invoke();

                return this;
            }
        }

        public class IfElseBuilder : BlockBuilder
        {
            public IfElseBuilder(Action addBlock, Action<string> appendLine, Action onDispose) : base(addBlock, appendLine, onDispose)
            {
            }

            public IfElseBuilder Else(string expression = "")
            {
                EndBlock?.Invoke();
                AppendLine?.Invoke($"else{(string.IsNullOrWhiteSpace(expression) ? string.Empty : $" if({expression})")}");
                AddBlock?.Invoke();

                return this;
            }
        }

        public static implicit operator string(CodeBuilder cb) => cb.ToString();
    }
}
