

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
namespace Cel.Parser;

public sealed class Options
{
    private readonly IDictionary<string, Macro> macros;

    private Options(int maxRecursionDepth, int errorRecoveryLimit, int expressionSizeCodePointLimit,
        IDictionary<string, Macro> macros)
    {
        this.MaxRecursionDepth = maxRecursionDepth;
        this.ErrorRecoveryLimit = errorRecoveryLimit;
        this.ExpressionSizeCodePointLimit = expressionSizeCodePointLimit;
        this.macros = macros;
    }

    public int MaxRecursionDepth { get; }

    public int ErrorRecoveryLimit { get; }

    public int ExpressionSizeCodePointLimit { get; }

    public Macro GetMacro(string name)
    {
        Macro macro = null;
        macros.TryGetValue(name, out macro);
        return macro;
    }

    public static Builder NewBuilder()
    {
        return new Builder();
    }

    public sealed class Builder
    {
        internal readonly IDictionary<string, Macro> macros = new Dictionary<string, Macro>();

        internal int errorRecoveryLimit = 30;

        internal int expressionSizeCodePointLimit = 100_000;

        internal int maxRecursionDepth = 250;

        internal Builder()
        {
        }

        public Builder MaxRecursionDepth(int maxRecursionDepth)
        {
            if (maxRecursionDepth < -1)
                throw new ArgumentException(string.Format(
                    "max recursion depth must be greater than or equal to -1: {0:D}", maxRecursionDepth));
            if (maxRecursionDepth == -1) maxRecursionDepth = int.MaxValue;

            this.maxRecursionDepth = maxRecursionDepth;
            return this;
        }

        public Builder ErrorRecoveryLimit(int errorRecoveryLimit)
        {
            if (errorRecoveryLimit < -1)
                throw new ArgumentException(string.Format(
                    "error recovery limit must be greater than or equal to -1: {0:D}", errorRecoveryLimit));
            if (errorRecoveryLimit == -1) errorRecoveryLimit = int.MaxValue;

            this.errorRecoveryLimit = errorRecoveryLimit;
            return this;
        }

        public Builder ExpressionSizeCodePointLimit(int expressionSizeCodePointLimit)
        {
            if (expressionSizeCodePointLimit < -1)
                throw new ArgumentException(string.Format(
                    "expression size code point limit must be greater than or equal to -1: {0:D}",
                    expressionSizeCodePointLimit));
            if (expressionSizeCodePointLimit == -1) expressionSizeCodePointLimit = int.MaxValue;

            this.expressionSizeCodePointLimit = expressionSizeCodePointLimit;
            return this;
        }

        public Builder Macros(params Macro[] macros)
        {
            return Macros(macros.ToList());
        }

        public Builder Macros(IList<Macro> macros)
        {
            foreach (var macro in macros) this.macros[macro.MacroKey()] = macro;

            return this;
        }

        public Options Build()
        {
            return new Options(maxRecursionDepth, errorRecoveryLimit,
                expressionSizeCodePointLimit, new Dictionary<string, Macro>(macros));
        }
    }
}