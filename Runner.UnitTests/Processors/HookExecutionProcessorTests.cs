﻿// Copyright 2015 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Strategy;
using Gauge.CSharp.Runner.UnitTests.Processors.Stubs;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    [TestFixture]
    public class HookExecutionProcessorTests
    {
        [BeforeScenario("Foo")]
        public void Foo()
        {
        }

        [BeforeScenario("Bar", "Baz")]
        public void Bar()
        {
        }

        [BeforeScenario("Foo", "Baz")]
        [TagAggregationBehaviour(TagAggregation.Or)]
        public void Baz()
        {
        }

        [BeforeScenario]
        public void Blah()
        {
        }

        /*
         * untagged hooks are executed for all.
         * Tags     | Methods
         * Foo      | Foo, Baz
         * Bar      | NONE
         * Baz      | Baz
         * Bar, Baz | Bar, Baz
         * Foo, Baz | Baz
         */

        private List<HookMethod> _hookMethods;

        [SetUp]
        public void Setup()
        {
            var mockSandbox = new Mock<ISandbox>();
            mockSandbox.Setup(sandbox => sandbox.TargetLibAssembly).Returns(typeof (Step).Assembly);

            _hookMethods = new List<HookMethod>
            {
                new HookMethod(GetType().GetMethod("Foo"), mockSandbox.Object),
                new HookMethod(GetType().GetMethod("Bar"), mockSandbox.Object),
                new HookMethod(GetType().GetMethod("Baz"), mockSandbox.Object),
                new HookMethod(GetType().GetMethod("Blah"), mockSandbox.Object)
            };
        }

        [Test]
        public void ShouldFetchAllHooksWhenNoTagsSpecified()
        {
            var applicableHooks = new HooksStrategy().GetApplicableHooks(new List<string>(), _hookMethods);

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(1, applicableHooks.Count());
        }

        [Test]
        public void ShouldFetchAllHooksWithSpecifiedTags()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"Foo"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(2, applicableHooks.Count);
            AssertEx.ContainsMethods(applicableHooks, "Baz", "Foo");
        }

        [Test]
        public void ShouldFetchAllHooksWithSpecifiedTagsWhenDoingAnd()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"Bar"}, _hookMethods);

            Assert.IsNotNull(applicableHooks);
            Assert.IsEmpty(applicableHooks);
        }

        [Test]
        public void ShouldFetchAnyHooksWithSpecifiedTagsWhenDoingOr()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"Baz"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(1, applicableHooks.Count);
            AssertEx.ContainsMethods(applicableHooks, "Baz");
        }

        [Test]
        public void ShouldFetchAHooksWithSpecifiedTagsWhenDoingAnd()
        {
            var applicableHooks =
                new HooksStrategy().GetTaggedHooks(new List<string> {"Baz", "Bar"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(2, applicableHooks.Count);
            AssertEx.ContainsMethods(applicableHooks, "Baz", "Bar");
        }

        [Test]
        public void ShouldFetchAHooksWithSpecifiedTagsWhenDoingOr()
        {
            var applicableHooks =
                new HooksStrategy().GetTaggedHooks(new List<string> {"Baz", "Foo"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(2, applicableHooks.Count);
            AssertEx.ContainsMethods(applicableHooks, "Baz", "Foo");
        }

        [Test]
        public void ShouldNotFetchAnyTaggedHooksWhenTagsAreASuperSet()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"Bar", "Blah"}, _hookMethods);

            Assert.IsNotNull(applicableHooks);
            Assert.IsEmpty(applicableHooks);
        }

        [Test]
        public void ShouldUseDefaultHooksStrategy()
        {
            var hooksStrategy = new TestHooksExecutionProcessor().GetHooksStrategy();

            Assert.IsInstanceOf<HooksStrategy>(hooksStrategy);
        }

        [Test]
        public void ShouldUseUntaggedHooksFirstStrategy()
        {
            var hooksStrategy = new TestUntaggedHooksFirstExecutionProcessor().GetHooksStrategy();

            Assert.IsInstanceOf<UntaggedHooksFirstStrategy>(hooksStrategy);
        }

        [Test]
        public void ShouldUseTaggedHooksFirstStrategy()
        {
            var hooksStrategy = new TestTaggedHooksFirstExecutionProcessor().GetHooksStrategy();

            Assert.IsInstanceOf<TaggedHooksFirstStrategy>(hooksStrategy);
        }
    }
}
