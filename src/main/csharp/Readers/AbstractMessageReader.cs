using System;
using System.Messaging;
using Apache.NMS.MSMQ;
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
    /// Abstract MSMQ message reader. Derived classes support various
    /// message filtering methods.
    /// </summary>
	public abstract class AbstractMessageReader : IMessageReader
	{
        protected MessageQueue messageQueue;
        protected IMessageConverter messageConverter;
        protected IMessageConverterEx messageConverterEx;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="messageQueue">The MSMQ message queue from which
        /// messages will be read.</param>
        /// <param name="messageConverter">A message converter for mapping
        /// MSMQ messages to NMS messages.</param>
        public AbstractMessageReader(MessageQueue messageQueue,
            IMessageConverter messageConverter)
        {
            this.messageQueue = messageQueue;

            this.messageConverter = messageConverter;
            this.messageConverterEx = (messageConverter as IMessageConverterEx);
        }

        /// <summary>
        /// Returns without removing (peeks) the first message in the queue
        /// referenced by this MessageQueue matching the selection criteria.
        /// The Peek method is synchronous, so it blocks the current thread
        /// until a message becomes available.
        /// </summary>
        /// <returns>Peeked message.</returns>
        public abstract IMessage Peek();

        /// <summary>
        /// Returns without removing (peeks) the first message in the queue
        /// referenced by this MessageQueue matching the selection criteria.
        /// The Peek method is synchronous, so it blocks the current thread
        /// until a message becomes available or the specified time-out occurs.
        /// </summary>
        /// <param name="timeSpan">Reception time-out.</param>
        /// <returns>Peeked message.</returns>
        public abstract IMessage Peek(TimeSpan timeSpan);

        /// <summary>
        /// Receives the first message available in the queue referenced by
        /// the MessageQueue matching the selection criteria.
        /// This call is synchronous, and blocks the current thread of execution
        /// until a message is available.
        /// </summary>
        /// <returns>Received message.</returns>
        public abstract IMessage Receive();

        /// <summary>
        /// Receives the first message available in the queue referenced by the
        /// MessageQueue matching the selection criteria, and waits until either
        /// a message is available in the queue, or the time-out expires.
        /// </summary>
        /// <param name="timeSpan">Reception time-out.</param>
        /// <returns>Received message.</returns>
        public abstract IMessage Receive(TimeSpan timeSpan);

        /// <summary>
        /// Receives the first message available in the transactional queue
        /// referenced by the MessageQueue matching the selection criteria.
        /// This call is synchronous, and blocks the current thread of execution
        /// until a message is available.
        /// </summary>
        /// <param name="transaction">Transaction.</param>
        /// <returns>Received message.</returns>
        public abstract IMessage Receive(MessageQueueTransaction transaction);

        /// <summary>
        /// Receives the first message available in the transactional queue
        /// referenced by the MessageQueue matching the selection criteria,
        /// and waits until either a message is available in the queue, or the
        /// time-out expires.
        /// </summary>
        /// <param name="timeSpan">Reception time-out.</param>
        /// <param name="transaction">Transaction.</param>
        /// <returns>Received message.</returns>
        public abstract IMessage Receive(TimeSpan timeSpan,
            MessageQueueTransaction transaction);

        /// <summary>
        /// Checks if an MSMQ message matches the selection criteria.
        /// </summary>
        /// <param name="message">MSMQ message.</param>
        /// <return>true if the message matches the selection criteria.</return>
        public abstract bool Matches(Message message);

        /// <summary>
        /// Converts an MSMQ message to an NMS message, using the converter
        /// specified at construction time.
        /// </summary>
        /// <param name="message">MSMQ message.</param>
        /// <return>NMS message.</return>
        protected IMessage Convert(Message message)
        {
            return message == null ? null : messageConverter.ToNmsMessage(message);
        }
	}
}
