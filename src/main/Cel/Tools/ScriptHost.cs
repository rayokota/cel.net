using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
using Google.Api.Expr.V1Alpha1;
using Google.Protobuf.Reflection;

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
namespace Cel.Tools;

/// <summary>
///     Manages <seealso cref="Script" /> instances, works like a factory to generate reusable scripts.
///     <para>
///         The current implementation is rather dumb, but it might be extended in the future to cache
///         <seealso cref="Script" /> instances returned by <seealso cref="GetOrCreateScript" />.
///     </para>
/// </summary>
public sealed class ScriptHost
{
    private readonly bool disableOptimize;
    private readonly ITypeRegistry registry;

    private ScriptHost(bool disableOptimize, ITypeRegistry registry)
    {
        this.disableOptimize = disableOptimize;
        this.registry = registry;
    }

    /// <summary>
    ///     Use <seealso cref="BuildScript(String)" />.
    /// </summary>
    [Obsolete]
    public Script GetOrCreateScript(string sourceText, IList<Decl> declarations, IList<object> types)
    {
        return BuildScript(sourceText).WithDeclarations(declarations).WithTypes(types).Build();
    }

    public ScriptBuilder BuildScript(string sourceText)
    {
        if (sourceText.Trim().Length == 0) throw new ArgumentException("No source code.");
        return new ScriptBuilder(this, sourceText);
    }

    public static Builder NewBuilder()
    {
        return new Builder();
    }

    public sealed class ScriptBuilder
    {
        private readonly IList<Decl> declarations = new List<Decl>();
        private readonly IList<ILibrary> libraries = new List<ILibrary>();
        private readonly ScriptHost outerInstance;

        private readonly string sourceText;
        private readonly IList<object> types = new List<object>();
        private string container;

        internal ScriptBuilder(ScriptHost outerInstance, string sourceText)
        {
            this.outerInstance = outerInstance;
            this.sourceText = sourceText;
        }

        public ScriptBuilder WithContainer(string container)
        {
            this.container = container;
            return this;
        }

        public ScriptBuilder WithDeclarations(params Decl[] declarations)
        {
            return WithDeclarations(new List<Decl>(declarations));
        }

        public ScriptBuilder WithDeclarations(IList<Decl> declarations)
        {
            ((List<Decl>)this.declarations).AddRange(declarations);
            return this;
        }

        public ScriptBuilder WithTypes(params object[] types)
        {
            return WithTypes(new List<object>(types));
        }

        public ScriptBuilder WithTypes(IList<object> types)
        {
            ((List<object>)this.types).AddRange(types);
            return this;
        }

        public ScriptBuilder WithLibraries(params ILibrary[] libraries)
        {
            return WithLibraries(new List<ILibrary>(libraries));
        }

        public ScriptBuilder WithLibraries(IList<ILibrary> libraries)
        {
            ((List<ILibrary>)this.libraries).AddRange(libraries);
            return this;
        }

        public Script Build()
        {
            IList<EnvOption> envOptions = new List<EnvOption>();
            envOptions.Add(LibraryOptions.StdLib());
            envOptions.Add(EnvOptions.Declarations(declarations));
            envOptions.Add(EnvOptions.Types(types));
            if (!ReferenceEquals(container, null)) envOptions.Add(EnvOptions.Container(container));
            ((List<EnvOption>)envOptions).AddRange(libraries.Select(l => LibraryOptions.Lib(l)).ToList());

            var env = Env.NewCustomEnv(outerInstance.registry, envOptions);

            var astIss = env.Parse(sourceText);
            if (astIss.HasIssues()) throw new ScriptCreateException("parse failed", astIss.Issues);
            var ast = astIss.Ast!;

            astIss = env.Check(ast);
            if (astIss.HasIssues()) throw new ScriptCreateException("check failed", astIss.Issues);

            IList<ProgramOption> programOptions = new List<ProgramOption>();
            if (!outerInstance.disableOptimize) programOptions.Add(ProgramOptions.EvalOptions(EvalOption.OptOptimize));

            var prg = env.Program(ast, ((List<ProgramOption>)programOptions).ToArray());

            return new Script(env, prg);
        }
    }

    public sealed class Builder
    {
        private bool disableOptimize;

        private ITypeRegistry? registry;

        internal Builder()
        {
        }

        /// <summary>
        ///     Call to instruct the built <seealso cref="ScriptHost" /> to disable script optimizations.
        /// </summary>
        /// <seealso cref="EvalOption.OptOptimize" />
        public Builder DisableOptimize()
        {
            disableOptimize = true;
            return this;
        }

        /// <summary>
        ///     Use the given <seealso cref="TypeRegistry" />.
        ///     <para>
        ///         The implementation will fall back to {@link
        ///         Cel.Common.Types.Pb.ProtoTypeRegistry}.
        ///     </para>
        /// </summary>
        public Builder Registry(ITypeRegistry? registry)
        {
            this.registry = registry;
            return this;
        }

        public ScriptHost Build()
        {
            var r = registry;
            if (r == null) r = ProtoTypeRegistry.NewRegistry();
            return new ScriptHost(disableOptimize, r);
        }
    }
}