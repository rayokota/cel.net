using System.Globalization;
using System.Text;
using Google.Api.Expr.V1Alpha1;

/*
 * Copyright (C) 2022 Robert Yokota
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Cel.Common.Debug;

public sealed class Debug
{
    /// <summary>
    ///     ToDebugString gives the unadorned string representation of the Expr.
    /// </summary>
    public string ToDebugString(Expr e)
    {
        return ToAdornedDebugString(e, new EmptyDebugAdorner());
    }

    /// <summary>
    ///     ToAdornedDebugString gives the adorned string representation of the Expr.
    /// </summary>
    public static string ToAdornedDebugString(Expr e, IAdorner adorner)
    {
        var w = new DebugWriter(adorner);
        w.Buffer(e);
        return w.ToString();
    }

    public static string FormatLiteral(Constant c)
    {
        switch (c.ConstantKindCase)
        {
            case Constant.ConstantKindOneofCase.BoolValue:
                return Convert.ToString(c.BoolValue).ToLower();
            case Constant.ConstantKindOneofCase.BytesValue:
                var sb = new StringBuilder();
                sb.Append("b\"");
                var bytes = c.BytesValue.ToByteArray();
                foreach (var b in bytes)
                {
                    var i = b & 0xff;
                    if (i >= 32 && i <= 127 && i != 34)
                        sb.Append((char)i);
                    else
                        switch (i)
                        {
                            case 7:
                                sb.Append("\\a");
                                break;
                            case 8:
                                sb.Append("\\b");
                                break;
                            case 9:
                                sb.Append("\\t");
                                break;
                            case 10:
                                sb.Append("\\n");
                                break;
                            case 11:
                                sb.Append("\\v");
                                break;
                            case 12:
                                sb.Append("\\f");
                                break;
                            case 13:
                                sb.Append("\\r");
                                break;
                            case '"':
                                sb.Append("\\\"");
                                break;
                            default:
                                sb.Append(string.Format("\\x{0:x2}", i));
                                break;
                        }
                }

                sb.Append("\"");
                return sb.ToString();
            case Constant.ConstantKindOneofCase.DoubleValue:
                var s = Convert.ToString(c.DoubleValue);
                if (s.EndsWith(".0", StringComparison.Ordinal)) return s.Substring(0, s.Length - 2);

                return s;
            case Constant.ConstantKindOneofCase.Int64Value:
                return Convert.ToString(c.Int64Value);
            case Constant.ConstantKindOneofCase.StringValue:
                sb = new StringBuilder();
                sb.Append('\"');
                s = c.StringValue;
                for (var i = 0; i < s.Length; i++)
                {
                    var ch = s[i];
                    switch (ch)
                    {
                        case (char)7: // BEL
                            sb.Append("\\a");
                            break;
                        case (char)11: // VT
                            sb.Append("\\v");
                            break;
                        case '\t':
                            sb.Append("\\t");
                            break;
                        case '\b':
                            sb.Append("\\b");
                            break;
                        case '\n':
                            sb.Append("\\n");
                            break;
                        case '\r':
                            sb.Append("\\r");
                            break;
                        case '\f':
                            sb.Append("\\f");
                            break;
                        case '\'':
                            sb.Append("'");
                            break;
                        case '\"':
                            sb.Append("\\\"");
                            break;
                        case '\\':
                            sb.Append("\\\\");
                            break;
                        default:
                            sb.Append(ch);
                            // TODO remove
                            /*
                              if (Char.IsLetter(ch))
                              {
                                sb.Append(ch);
                              }
                              else
                              {
                                sb.Append(String.Format("\\u{0:x4}", ((int) ch) & 0xffff));
                              }
                              */
                            break;
                    }
                }

                sb.Append('\"');
                return sb.ToString();
            case Constant.ConstantKindOneofCase.Uint64Value:
                return Convert.ToString(c.Uint64Value) + "u";
            case Constant.ConstantKindOneofCase.NullValue:
                return "null";
            default:
                throw new ArgumentException("" + c);
        }
    }

    // TODO remove
    private static bool IsCharDefined(char c)
    {
        var surrogate = char.ConvertFromUtf32(c);
        return char.GetUnicodeCategory(surrogate, 0) != UnicodeCategory.OtherNotAssigned;
    }

    /// <summary>
    ///     IAdorner returns debug metadata that will be tacked on to the string representation of an
    ///     expression.#
    /// </summary>
    public interface IAdorner
    {
        /// <summary>
        ///     GetMetadata for the input context.
        /// </summary>
        string GetMetadata(object ctx);
    }

    internal sealed class EmptyDebugAdorner : IAdorner
    {
        public string GetMetadata(object e)
        {
            return "";
        }
    }

    /// <summary>
    ///     debugWriter is used to print out pretty-printed debug strings.
    /// </summary>
    public sealed class DebugWriter
    {
        private readonly IAdorner adorner;
        private readonly StringBuilder buffer;
        private int indent;
        private bool lineStart;

        public DebugWriter(IAdorner a)
        {
            adorner = a;
            buffer = new StringBuilder();
            indent = 0;
            lineStart = true;
        }

        internal void Buffer(Expr e)
        {
            if (e == null) return;

            switch (e.ExprKindCase)
            {
                case Expr.ExprKindOneofCase.ConstExpr:
                    Append(FormatLiteral(e.ConstExpr));
                    break;
                case Expr.ExprKindOneofCase.IdentExpr:
                    Append(e.IdentExpr.Name);
                    break;
                case Expr.ExprKindOneofCase.SelectExpr:
                    AppendSelect(e.SelectExpr);
                    break;
                case Expr.ExprKindOneofCase.CallExpr:
                    AppendCall(e.CallExpr);
                    break;
                case Expr.ExprKindOneofCase.ListExpr:
                    AppendList(e.ListExpr);
                    break;
                case Expr.ExprKindOneofCase.StructExpr:
                    AppendStruct(e.StructExpr);
                    break;
                case Expr.ExprKindOneofCase.ComprehensionExpr:
                    AppendComprehension(e.ComprehensionExpr);
                    break;
                case Expr.ExprKindOneofCase.None:
                    throw new InvalidOperationException("Expr w/o kind");
            }

            Adorn(e);
        }

        internal void AppendSelect(Expr.Types.Select sel)
        {
            Buffer(sel.Operand);
            Append(".");
            Append(sel.Field);
            if (sel.TestOnly) Append("~test-only~");
        }

        internal void AppendCall(Expr.Types.Call call)
        {
            if (call.Target != null)
            {
                Buffer(call.Target);
                Append(".");
            }

            Append(call.Function);
            Append("(");
            if (call.Args.Count > 0)
            {
                AddIndent();
                AppendLine();
                for (var i = 0; i < call.Args.Count; i++)
                {
                    if (i > 0)
                    {
                        Append(",");
                        AppendLine();
                    }

                    Buffer(call.Args[i]);
                }

                RemoveIndent();
                AppendLine();
            }

            Append(")");
        }

        internal void AppendList(Expr.Types.CreateList list)
        {
            Append("[");
            if (list.Elements.Count > 0)
            {
                AppendLine();
                AddIndent();
                for (var i = 0; i < list.Elements.Count; i++)
                {
                    if (i > 0)
                    {
                        Append(",");
                        AppendLine();
                    }

                    Buffer(list.Elements[i]);
                }

                RemoveIndent();
                AppendLine();
            }

            Append("]");
        }

        internal void AppendStruct(Expr.Types.CreateStruct obj)
        {
            if (obj.MessageName.Length > 0)
                AppendObject(obj);
            else
                AppendMap(obj);
        }

        internal void AppendObject(Expr.Types.CreateStruct obj)
        {
            Append(obj.MessageName);
            Append("{");
            if (obj.Entries.Count > 0)
            {
                AppendLine();
                AddIndent();
                for (var i = 0; i < obj.Entries.Count; i++)
                {
                    if (i > 0)
                    {
                        Append(",");
                        AppendLine();
                    }

                    var entry = obj.Entries[i];
                    Append(entry.FieldKey);
                    Append(":");
                    Buffer(entry.Value);
                    Adorn(entry);
                }

                RemoveIndent();
                AppendLine();
            }

            Append("}");
        }

        internal void AppendMap(Expr.Types.CreateStruct obj)
        {
            Append("{");
            if (obj.Entries.Count > 0)
            {
                AppendLine();
                AddIndent();
                for (var i = 0; i < obj.Entries.Count; i++)
                {
                    if (i > 0)
                    {
                        Append(",");
                        AppendLine();
                    }

                    var entry = obj.Entries[i];
                    Buffer(entry.MapKey);
                    Append(":");
                    Buffer(entry.Value);
                    Adorn(entry);
                }

                RemoveIndent();
                AppendLine();
            }

            Append("}");
        }

        internal void AppendComprehension(Expr.Types.Comprehension comprehension)
        {
            Append("__comprehension__(");
            AddIndent();
            AppendLine();
            Append("// Variable");
            AppendLine();
            Append(comprehension.IterVar);
            Append(",");
            AppendLine();
            Append("// Target");
            AppendLine();
            Buffer(comprehension.IterRange);
            Append(",");
            AppendLine();
            Append("// Accumulator");
            AppendLine();
            Append(comprehension.AccuVar);
            Append(",");
            AppendLine();
            Append("// Init");
            AppendLine();
            Buffer(comprehension.AccuInit);
            Append(",");
            AppendLine();
            Append("// LoopCondition");
            AppendLine();
            Buffer(comprehension.LoopCondition);
            Append(",");
            AppendLine();
            Append("// LoopStep");
            AppendLine();
            Buffer(comprehension.LoopStep);
            Append(",");
            AppendLine();
            Append("// Result");
            AppendLine();
            Buffer(comprehension.Result);
            Append(")");
            RemoveIndent();
        }

        internal void Append(string s)
        {
            DoIndent();
            buffer.Append(s);
        }

        internal void AppendFormat(string f, params object[] args)
        {
            Append(string.Format(f, args));
        }

        internal void DoIndent()
        {
            if (lineStart)
            {
                lineStart = false;
                for (var i = 0; i < indent; i++) buffer.Append("  ");
            }
        }

        internal void Adorn(object e)
        {
            Append(adorner.GetMetadata(e));
        }

        internal void AppendLine()
        {
            buffer.Append("\n");
            lineStart = true;
        }

        internal void AddIndent()
        {
            indent++;
        }

        internal void RemoveIndent()
        {
            indent--;
            if (indent < 0) throw new InvalidOperationException("negative indent");
        }

        public override string ToString()
        {
            return buffer.ToString();
        }
    }
}