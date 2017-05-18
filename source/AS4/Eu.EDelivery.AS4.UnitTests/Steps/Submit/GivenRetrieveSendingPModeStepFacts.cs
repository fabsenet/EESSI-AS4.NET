﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Steps.Submit;
using Eu.EDelivery.AS4.UnitTests.Common;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Submit
{
    /// <summary>
    /// Testing <see cref="RetrieveSendingPModeStep" />
    /// </summary>
    public class GivenRetrieveSendingPModeStepFacts
    {
        private readonly Mock<IConfig> _mockedConfig;
        private readonly string _pmodeId;

        private RetrieveSendingPModeStep _step;

        public GivenRetrieveSendingPModeStepFacts()
        {
            _step = new RetrieveSendingPModeStep(StubConfig.Instance);
            _pmodeId = "01-pmode";
            _mockedConfig = new Mock<IConfig>();
            _mockedConfig.Setup(c => c.GetSendingPMode(It.IsAny<string>())).Returns(GetStubWrongProcessingMode());
        }

        private SendingProcessingMode GetStubWrongProcessingMode()
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            return new SendingProcessingMode {Id = _pmodeId, PushConfiguration = new PushConfiguration()};
        }

        private SubmitMessage GetStubSubmitMessage()
        {
            return new SubmitMessage
            {
                Collaboration = new CollaborationInfo {AgreementRef = new Agreement {PModeId = _pmodeId}}
            };
        }

        /// <summary>
        /// Testing if the Step fails
        /// for the "Execute" Method
        /// </summary>
        public class GivenInalidArgumentsForExecute : GivenRetrieveSendingPModeStepFacts
        {
            [Fact]
            public async Task ThenExecuteMethodRetrievesPModeFailsWithInvalidPModeAsync()
            {
                // Arrange
                var internalMessage = new InternalMessage(GetStubSubmitMessage());
                _step = new RetrieveSendingPModeStep(_mockedConfig.Object);

                // Act / Assert
                await Assert.ThrowsAsync<AS4Exception>(
                    () => _step.ExecuteAsync(internalMessage, CancellationToken.None));
            }
        }
    }
}