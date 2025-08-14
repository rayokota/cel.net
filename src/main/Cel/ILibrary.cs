﻿using Cel.Interpreter.Functions;
using Cel.Parser;

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
namespace Cel;

/// <summary>
///     ILibrary provides a collection of EnvOption and ProgramOption values used to confiugre a CEL
///     environment for a particular use case or with a related set of functionality.
///     <para>
///         Note, the ProgramOption values provided by a library are expected to be static and not vary
///         between calls to Env.Program(). If there is a need for such dynamic configuration, prefer to
///         configure these options outside the Library and within the Env.Program() call directly.
///     </para>
/// </summary>
public interface ILibrary
{
    /// <summary>
    ///     CompileOptions returns a collection of funcitional options for configuring the Parse / Check
    ///     environment.
    /// </summary>
    IList<EnvOption> CompileOptions { get; }

    /// <summary>
    ///     ProgramOptions returns a collection of functional options which should be included in every
    ///     Program generated from the Env.Program() call.
    /// </summary>
    IList<ProgramOption> ProgramOptions { get; }

    /// <summary>
    ///     Lib creates an EnvOption out of a Library, allowing libraries to be provided as functional
    ///     args, and to be linked to each other.
    /// </summary>
    

    /// <summary>
    ///     StdLib returns an EnvOption for the standard library of CEL functions and macros.
    /// </summary>
    
}

public static class LibraryOptions
{
    public static EnvOption Lib(ILibrary l)
    {
        return e =>
        {
            foreach (var opt in l.CompileOptions)
            {
                e = opt(e);
                if (e == null)
                    throw new NullReferenceException(string.Format("env option of type '{0}' returned null",
                        opt.GetType()));
            }

            e.AddProgOpts(l.ProgramOptions);
            return e;
        };
    }

    public static EnvOption StdLib()
    {
        return Lib(new StdLibrary());
    }
}

/// <summary>
///     stdLibrary implements the Library interface and provides functional options for the core CEL
///     features documented in the specification.
/// </summary>
public sealed class StdLibrary : ILibrary
{
    /// <summary>
    ///     EnvOptions returns options for the standard CEL function declarations and macros.
    /// </summary>
    public IList<EnvOption> CompileOptions =>
        new List<EnvOption>
        {
            EnvOptions.Declarations(Checker.Checker.StandardDeclarations),
            EnvOptions.Macros(Macro.AllMacros)
        };

    /// <summary>
    ///     ProgramOptions returns function implementations for the standard CEL functions.
    /// </summary>
    public IList<ProgramOption> ProgramOptions => new List<ProgramOption>
        { global::Cel.ProgramOptions.Functions(Overload.StandardOverloads()) };
}