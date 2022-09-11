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

using Google.Api.Expr.V1Alpha1;
using NUnit.Framework;

namespace Cel.Common.Containers;

public class ContainerTest
{
    [Test]
    public virtual void ResolveCandidateNames()
    {
        var c = Container.NewContainer(Container.Name("a.b.c.M.N"))!;
        var names = c.ResolveCandidateNames("R.s");
        string[] want = { "a.b.c.M.N.R.s", "a.b.c.M.R.s", "a.b.c.R.s", "a.b.R.s", "a.R.s", "R.s" };
        Assert.That(names, Is.EquivalentTo(want));
    }

    [Test]
    public virtual void ResolveCandidateNamesFullyQualifiedName()
    {
        var c = Container.NewContainer(Container.Name("a.b.c.M.N"))!;
        // The leading '.' indicates the name is already fully-qualified.
        var names = c.ResolveCandidateNames(".R.s");
        string[] want = { "R.s" };
        Assert.That(names, Is.EquivalentTo(want));
    }

    [Test]
    public virtual void ResolveCandidateNamesEmptyContainer()
    {
        var names = Container.DefaultContainer.ResolveCandidateNames("R.s");
        string[] want = { "R.s" };
        Assert.That(names, Is.EquivalentTo(want));
    }

    [Test]
    public virtual void Abbrevs()
    {
        var abbr = Container.DefaultContainer.Extend(Container.Abbrevs("my.alias.R"))!;
        var names = abbr.ResolveCandidateNames("R");
        string[] want = { "my.alias.R" };
        Assert.That(names, Is.EquivalentTo(want));
        var c = Container.NewContainer(Container.Name("a.b.c"), Container.Abbrevs("my.alias.R"))!;
        names = c.ResolveCandidateNames("R");
        want = new[] { "my.alias.R" };
        Assert.That(names, Is.EquivalentTo(want));
        names = c.ResolveCandidateNames("R.S.T");
        want = new[] { "my.alias.R.S.T" };
        Assert.That(names, Is.EquivalentTo(want));
        names = c.ResolveCandidateNames("S");
        want = new[] { "a.b.c.S", "a.b.S", "a.S", "S" };
        Assert.That(names, Is.EquivalentTo(want));
    }

    [Test]
    public virtual void AliasingErrors()
    {
        Assert.That(() => Container.NewContainer(Container.Abbrevs("my.alias.R", "yer.other.R")),
            Throws.Exception.TypeOf(typeof(ArgumentException)));

        Assert
            .That(() =>
                    Container.NewContainer(Container.Name("a.b.c.M.N"),
                        Container.Abbrevs("my.alias.a", "yer.other.b")),
                Throws.Exception.TypeOf(typeof(ArgumentException)));

        Assert.That(() => Container.NewContainer(Container.Abbrevs(".bad")),
            Throws.Exception.TypeOf(typeof(ArgumentException)));

        Assert.That(() => Container.NewContainer(Container.Abbrevs("bad.alias.")),
            Throws.Exception.TypeOf(typeof(ArgumentException)));

        Assert.That(() => Container.NewContainer(Container.Alias("a", "b")),
            Throws.Exception.TypeOf(typeof(ArgumentException)));

        Assert.That(() => Container.NewContainer(Container.Alias("my.alias", "b.c")),
            Throws.Exception.TypeOf(typeof(ArgumentException)));

        Assert.That(() => Container.NewContainer(Container.Alias(".my.qual.name", "a")),
            Throws.Exception.TypeOf(typeof(ArgumentException)));

        Assert.That(() => Container.NewContainer(Container.Alias(".my.qual.name", "a")),
            Throws.Exception.TypeOf(typeof(ArgumentException)));
    }

    [Test]
    public virtual void ExtendAlias()
    {
        var c = Container.DefaultContainer.Extend(Container.Alias("test.alias", "alias"))!;
        Assert.That(c.AliasSet(), Is.EquivalentTo(new Dictionary<string, string>
        {
            { "alias", "test.alias" }
        }));

        c = c.Extend(Container.Name("with.container"));
        Assert.That(c.Name(), Is.EqualTo("with.container"));
        Assert.That(c.AliasSet(), Is.EquivalentTo(new Dictionary<string, string> { { "alias", "test.alias" } }));
    }

    [Test]
    public virtual void ExtendName()
    {
        var c = Container.DefaultContainer.Extend(Container.Name(""))!;
        Assert.That(c.Name(), Is.Empty);
        c = Container.DefaultContainer.Extend(Container.Name("hello.container"));
        Assert.That(c.Name(), Is.EqualTo("hello.container"));
        c = c.Extend(Container.Name("goodbye.container"));
        Assert.That(c.Name(), Is.EqualTo("goodbye.container"));
        var cc = c;
        Assert.That(() => cc.Extend(Container.Name(".bad.container")),
            Throws.Exception.TypeOf(typeof(ArgumentException)));
    }

    [Test]
    public virtual void ToQualifiedName()
    {
        var id = new Expr.Types.Ident();
        id.Name = "var";
        var ident = new Expr();
        ident.Id = 0;
        ident.IdentExpr = id;
        var idName = Container.ToQualifiedName(ident);
        Assert.That(idName, Is.EqualTo("var"));

        var s = new Expr.Types.Select();
        s.Operand = ident;
        s.Field = "qualifier";
        var sel = new Expr();
        sel.Id = 0;
        sel.SelectExpr = s;
        var qualName = Container.ToQualifiedName(sel);
        Assert.That(qualName, Is.EqualTo("var.qualifier"));

        s = new Expr.Types.Select();
        s.Operand = ident;
        s.Field = "qualifier";
        s.TestOnly = true;
        sel = new Expr();
        sel.Id = 0;
        sel.SelectExpr = s;

        Assert.That(Container.ToQualifiedName(sel), Is.Null);

        var call = new Expr.Types.Call();
        call.Function = "!_";
        call.Args.Add(ident);
        var unary = new Expr();
        unary.Id = 0;
        unary.CallExpr = call;
        sel = new Expr();
        s = new Expr.Types.Select();
        s.Operand = unary;
        s.Field = "qualifier";
        sel = new Expr();
        sel.Id = 0;
        sel.SelectExpr = s;
        Assert.That(Container.ToQualifiedName(sel), Is.Null);
    }
}