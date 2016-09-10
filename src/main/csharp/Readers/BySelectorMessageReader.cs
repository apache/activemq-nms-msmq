using System;
using System.Messaging;
using Apache.NMS.MSMQ;
using Apache.NMS;
using Apache.NMS.Selector;
/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Apache.NMS.MSMQ.Readers
{
    /// <summary>
    /// MSMQ message reader, returning messages matching the specified
    /// selector.
    /// </summary>
	public class BySelectorMessageReader : AbstractMessageReader
	{
        private string selector;
        private MessageEvaluationContext evaluationContext;
        private IBooleanExpression selectionExpression;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="messageQueue">The MSMQ message queue from which
        /// messages will be read.</param>
        /// <param name="messageConverter">A message converter for mapping
        /// MSMQ messages to NMS messages.</param>
        /// <param name="selector">The selector string.</param>
        public BySelectorMessageReader(MessageQueue messageQueue,
            IMessageConverter messageConverter, string selector)
            : base(messageQueue, messageConverter)
        {
            this.selector = selector;

            SelectorParser selectorParser = new SelectorParser();
            selectionExpression = selectorParser.Parse(selector);

            evaluationContext = new MessageEvaluationContext(null);
        }

        /// <summary>
        /// Returns without removing (peeks) the first message in the queue
        /// referenced by this MessageQueue matching the selection criteria.
        /// The Peek method is synchronous, so it blocks the current thread
        /// until a message becomes available.
        /// </summary>
        /// <returns>Peeked message.</returns>
        public override IMessage Peek()
        {
            return InternalPeek(DateTime.MaxValue, true);
        }

        /// <summary>
        /// Returns without removing (peeks) the first message in the queue
        /// referenced by this MessageQueue matching the selection criteria.
        /// The Peek method is synchronous, so it blocks the current thread
        /// until a message becomes available or the specified time-out occurs.
        /// </summary>
        /// <param name="timeSpan">Reception time-out.</param>
        /// <returns>Peeked message.</returns>
        public override IMessage Peek(TimeSpan timeSpan)
        {
            DateTime maxTime = DateTime.Now + timeSpan;
            return InternalPeek(maxTime, true);
        }

        /// <summary>
        /// Receives the first message available in the queue referenced by
        /// the MessageQueue matching the selection criteria.
        /// This call is synchronous, and blocks the current thread of execution
        /// until a message is available.
        /// </summary>
        /// <returns>Received message.</returns>
        public override IMessage Receive()
        {
            return InternalReceive(DateTime.MaxValue, null);
        }

        /// <summary>
        /// Receives the first message available in the queue referenced by the
        /// MessageQueue matching the selection criteria, and waits until either
        /// a message is available in the queue, or the time-out expires.
        /// </summary>
        /// <param name="timeSpan">Reception time-out.</param>
        /// <returns>Received message.</returns>
        public override IMessage Receive(TimeSpan timeSpan)
        {
            return InternalReceive(DateTime.Now + timeSpan, null);
        }

        /// <summary>
        /// Receives the first message available in the transactional queue
        /// referenced by the MessageQueue matching the selection criteria.
        /// This call is synchronous, and blocks the current thread of execution
        /// until a message is available.
        /// </summary>
        /// <param name="transaction">Transaction.</param>
        /// <returns>Received message.</returns>
        public override IMessage Receive(MessageQueueTransaction transaction)
        {
            return InternalReceive(DateTime.MaxValue, transaction);
        }

        /// <summary>
        /// Receives the first message available in the transactional queue
        /// referenced by the MessageQueue matching the selection criteria,
        /// and waits until either a message is available in the queue, or the
        /// time-out expires.
        /// </summary>
        /// <param name="timeSpan">Reception time-out.</param>
        /// <param name="transaction">Transaction.</param>
        /// <returns>Received message.</returns>
        public override IMessage Receive(TimeSpan timeSpan,
            MessageQueueTransaction transaction)
        {
            return InternalReceive(DateTime.Now + timeSpan, transaction);
        }

        /// <summary>
        /// Receives the first message available in the transactional queue
        /// referenced by the MessageQueue matching the selection criteria,
        /// and waits until either a message is available in the queue, or the
        /// time-out expires.
        /// </summary>
        /// <param name="maxTime">Reception time-out.</param>
        /// <param name="transaction">Transaction.</param>
        /// <returns>Received message.</returns>
        public IMessage InternalReceive(DateTime maxTime,
            MessageQueueTransaction transaction)
        {
            // In a shared connection / multi-consumer context, the message may
            // have been consumed by another client, after it was peeked but
            // before it was peeked by this client. Hence the loop.
            // (not sure it can be shared AND transactional, though).
            while(true)
            {
                IMessage peekedMessage = InternalPeek(maxTime, false);

                if(peekedMessage == null)
                {
                    return null;
                }

                try
                {
                    long lookupId = peekedMessage.Properties.GetLong("LookupId");

                    Message message = (transaction == null ?
                        messageQueue.ReceiveByLookupId(lookupId) :
                        messageQueue.ReceiveByLookupId(
                            MessageLookupAction.Current, lookupId, transaction));

                    return Convert(message);
                }
                catch(InvalidOperationException)
                {
                    // TODO: filter exceptions, catch only exceptions due to                    
                    // unknown lookup id.
                }
            }
        }

        /// <summary>
        /// Returns without removing (peeks) the first message in the queue
        /// referenced by this MessageQueue, matching the selection criteria.
        /// </summary>
        /// <param name="maxTime">Reception time-out.</param>
        /// <param name="convertBody">true if message body should be converted.</param>
        /// <returns>Peeked message.</returns>
        private IMessage InternalPeek(DateTime maxTime, bool convertBody)
        {
            TimeSpan timeSpan = maxTime - DateTime.Now;
            if(timeSpan <= TimeSpan.Zero)
            {
                timeSpan = TimeSpan.Zero;
            }

            using(Cursor cursor = messageQueue.CreateCursor())
            {
                PeekAction action = PeekAction.Current;
                while(true)
                {
                    Message msmqMessage = null;

                    try
                    {
                        msmqMessage = messageQueue.Peek(timeSpan, cursor, action);
                    }
                    catch(MessageQueueException exc)
                    {
                        if(exc.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                        {
                            return null;
                        }
                        throw exc;
                    }

                    IMessage nmsMessage = InternalMatch(msmqMessage, convertBody);

                    if(nmsMessage != null)
                    {
                        return nmsMessage;
                    }

                    action = PeekAction.Next;
                }
            }
        }

        /// <summary>
        /// Checks if an MSMQ message matches the selection criteria. If matched
        /// the method returns the converted NMS message. Else it returns null.
        /// </summary>
        /// <param name="message">The MSMQ message to check.</param>
        /// <param name="convertBody">true if the message body should be
        /// converted.</param>
        /// <returns>The matching message converted to NMS, or null.</returns>
        private IMessage InternalMatch(Message message, bool convertBody)
        {
            if(messageConverterEx == null)
            {
                IMessage nmsMessage = messageConverter.ToNmsMessage(message);

                evaluationContext.Message = nmsMessage;

                if(selectionExpression.Matches(evaluationContext))
                {
                    return nmsMessage;
                }
            }
            else
            {
                // This version converts the message body only for those
                // messages matching the selection criteria.
                // Relies on MessageConverterEx for partial conversions.
                IMessage nmsMessage = messageConverterEx.ToNmsMessage(
                    message, false);

                evaluationContext.Message = nmsMessage;

                if(selectionExpression.Matches(evaluationContext))
                {
                    if(convertBody)
                    {
                        messageConverterEx.ConvertMessageBodyToNMS(
                            message, nmsMessage);
                    }

                    return nmsMessage;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if an MSMQ message matches the selection criteria.
        /// </summary>
        /// <param name="message">MSMQ message.</param>
        /// <return>true if the message matches the selection criteria.</return>
        public override bool Matches(Message message)
        {
            IMessage nmsMessage = messageConverterEx == null ?
                messageConverter.ToNmsMessage(message) :
                messageConverterEx.ToNmsMessage(message, false);

            evaluationContext.Message = nmsMessage;

            return selectionExpression.Matches(evaluationContext);
        }
	}
}
