﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Repositories
{
    /// <summary>
    /// Testing the <see cref="DatastoreRepository"/>
    /// </summary>
    public class GivenDatastoreRepositoryFacts : GivenDatastoreFacts
    {
        public class GivenValidArguments : GivenDatastoreRepositoryFacts
        {
            [Theory, InlineData("shared-id")]
            public void ThenGetInMessageSucceeded(string sharedId)
            {
                // Arrange
                var inMessage = new InMessage() { EbmsMessageId = sharedId };
                InsertInMessage(inMessage);

                using (var context = this.GetDataStoreContext())
                {
                    var repository = new DatastoreRepository(context);

                    // Act
                    InMessage resultMessage = repository.GetInMessageById(inMessage.EbmsMessageId);
                    // Assert
                    Assert.NotNull(resultMessage);
                }
            }

            [Theory, InlineData("shared-id")]
            public void ThenGetOutMessageSucceeded(string sharedId)
            {
                // Arrange
                var outMessage = new OutMessage() { EbmsMessageId = sharedId };
                InsertOutMessage(outMessage);

                using (var context = this.GetDataStoreContext())
                {
                    var repository = new DatastoreRepository(context);

                    // Act
                    OutMessage resultMessage = repository.GetOutMessageById(outMessage.EbmsMessageId);
                    // Assert
                    Assert.NotNull(resultMessage);
                }
            }

            [Theory, InlineData("shared-id")]
            public async Task ThenInsertInMessageSucceedsAsync(string sharedId)
            {
                // Arrange
                var inMessage = new InMessage() { EbmsMessageId = sharedId };

                using (var context = this.GetDataStoreContext())
                {
                    // Act
                    await new DatastoreRepository(context).InsertInMessageAsync(inMessage);
                }

                // Assert
                AssertInMessage(inMessage.EbmsMessageId, Assert.NotNull);
            }

            [Theory, InlineData("shared-id")]
            public async Task ThenInsertInExceptionSucceedsAsync(string sharedId)
            {
                // Arrange
                var inException = new InException() { EbmsRefToMessageId = sharedId };
                // Act

                using (var context = this.GetDataStoreContext())
                {
                    await new DatastoreRepository(context).InsertInExceptionAsync(inException);
                    // Assert
                }

                AssertInException(inException.EbmsRefToMessageId, Assert.NotNull);
            }

            [Theory, InlineData("shared-id")]
            public async Task ThenInsertOutMessageSucceedsAsync(string sharedId)
            {
                // Arrange
                var outMessage = new OutMessage() { EbmsMessageId = sharedId };
                // Act
                using (var context = this.GetDataStoreContext())
                {
                    await new DatastoreRepository(context).InsertOutMessageAsync(outMessage);
                }

                // Assert
                AssertOutMessage(outMessage.EbmsMessageId, Assert.NotNull);
            }

            [Theory, InlineData("shared-id")]
            public async Task ThenInsertOutExceptionSucceedsAsync(string sharedId)
            {
                // Arrange
                var outException = new OutException() { EbmsRefToMessageId = sharedId };
                // Act
                using (var context = this.GetDataStoreContext())
                {
                    await new DatastoreRepository(context).InsertOutExceptionAsync(outException);
                }
                // Assert
                AssertOutException(outException.EbmsRefToMessageId, Assert.NotNull);
            }

            [Theory, InlineData("share-id")]
            public async Task ThenUpdateInMessageSucceedsAsync(string sharedId)
            {
                // Arrange
                var inMessage = new InMessage
                {
                    EbmsMessageId = sharedId,
                    Operation = Operation.ToBeDelivered
                };
                InsertInMessage(inMessage);
                // Act
                using (var context = this.GetDataStoreContext())
                {
                    await new DatastoreRepository(context).UpdateInMessageAsync(inMessage.EbmsMessageId,
                          m => m.Operation = Operation.Delivered);
                }
                // Assert
                AssertInMessage(inMessage.EbmsMessageId,
                    m => Assert.Equal(Operation.Delivered, m.Operation));
            }

            private void InsertInMessage(InMessage inMessage)
            {
                using (var context = this.GetDataStoreContext())
                {
                    context.InMessages.Add(inMessage);
                    context.SaveChanges();
                }
            }

            private void AssertInMessage(string messageId, Action<InMessage> assertAction)
            {
                using (var context = new DatastoreContext(base.Options))
                {
                    InMessage inMessage = context.InMessages
                        .FirstOrDefault(m => m.EbmsMessageId.Equals(messageId));
                    assertAction(inMessage);
                }
            }

            private void AssertInException(string messageId, Action<InException> assertAction)
            {
                using (var context = this.GetDataStoreContext())
                {
                    InException inException = context.InExceptions
                        .FirstOrDefault(m => m.EbmsRefToMessageId.Equals(messageId));
                    assertAction(inException);
                }
            }

            [Theory, InlineData("shared-id")]
            public async Task ThenUpdateOutMessageSucceedsAsync(string sharedId)
            {
                // Arrange
                var outMessage = new OutMessage
                {
                    EbmsMessageId = sharedId,
                    Operation = Operation.ToBeSent
                };
                InsertOutMessage(outMessage);
                // Act
                using (var context = this.GetDataStoreContext())
                {
                    await new DatastoreRepository(context).UpdateOutMessageAsync(outMessage.EbmsMessageId,
                    m => m.Operation = Operation.Sent);
                }
                // Assert
                AssertOutMessage(outMessage.EbmsMessageId,
                    m => Assert.Equal(Operation.Sent, m.Operation));

            }

            private void InsertOutMessage(OutMessage outMessage)
            {
                using (var context = new DatastoreContext(base.Options))
                {
                    context.OutMessages.Add(outMessage);
                    context.SaveChanges();
                }
            }

            private void AssertOutMessage(string messageId, Action<OutMessage> assertAction)
            {
                using (var contex = new DatastoreContext(base.Options))
                {
                    OutMessage outMessage = contex.OutMessages
                        .FirstOrDefault(m => m.EbmsMessageId.Equals(messageId));
                    assertAction(outMessage);
                }
            }

            private void AssertOutException(string messageId, Action<OutException> assertAction)
            {
                using (var contex = new DatastoreContext(base.Options))
                {
                    OutException outException = contex.OutExceptions
                        .FirstOrDefault(m => m.EbmsRefToMessageId.Equals(messageId));
                    assertAction(outException);
                }
            }
        }
    }
}
