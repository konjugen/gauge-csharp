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
using System.Reflection;
using Gauge.CSharp.Runner.Processors;
using Gauge.Messages;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    class StepExecutionStartingProcessorTests
    {
        [Test]
        public void ShouldExtendFromHooksExecutionProcessor()
        {
            AssertEx.InheritsFrom<HookExecutionProcessor, StepExecutionStartingProcessor>();
            AssertEx.DoesNotInheritsFrom<TaggedHooksFirstExecutionProcessor, StepExecutionStartingProcessor>();
            AssertEx.DoesNotInheritsFrom<UntaggedHooksFirstExecutionProcessor, StepExecutionStartingProcessor>();
        }

        [Test]
        public void ShouldClearExistingGaugeMessages()
        {
            var methodExecutor = new Mock<IMethodExecutor>();

            var request = Message.CreateBuilder()
                .SetMessageId(20)
                .SetMessageType(Message.Types.MessageType.StepExecutionStarting)
                .SetStepExecutionStartingRequest(StepExecutionStartingRequest.DefaultInstance)
                .Build();

            var protoExecutionResultBuilder = ProtoExecutionResult.CreateBuilder().SetExecutionTime(0).SetFailed(false);
            methodExecutor.Setup( executor => executor.ExecuteHooks(It.IsAny<IEnumerable<MethodInfo>>(),
                        It.IsAny<ExecutionInfo>()))
                          .Returns(protoExecutionResultBuilder);
            var hookRegistry = new Mock<IHookRegistry>();
            hookRegistry.Setup(registry => registry.BeforeStepHooks).Returns(new HashSet<HookMethod>());

            new StepExecutionStartingProcessor(hookRegistry.Object, methodExecutor.Object).Process(request);

            methodExecutor.Verify(executor => executor.GetAllPendingMessages(), Times.Once);
        }
    }
}
