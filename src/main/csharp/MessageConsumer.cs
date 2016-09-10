using System;
using System.Messaging;
using System.Threading;
using Apache.NMS.Util;
using Apache.NMS.MSMQ.Readers;
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

namespace Apache.NMS.MSMQ
{
    /// <summary>
    /// An object capable of receiving messages from some destination.
    /// </summary>
    public class MessageConsumer : IMessageConsumer
    {
        protected TimeSpan zeroTimeout = new TimeSpan(0);

        private readonly Session session;
        private readonly AcknowledgementMode acknowledgementMode;
        private MessageQueue messageQueue;

        private ConsumerTransformerDelegate consumerTransformer;
        public ConsumerTransformerDelegate ConsumerTransformer
        {
            get { return this.consumerTransformer; }
            set { this.consumerTransformer = value; }
        }

        private IMessageReader reader;

        /// <summary>
        /// Constructs a message consumer on the specified queue.
        /// </summary>
        /// <param name="session">The messaging session.</param>
        /// <param name="acknowledgementMode">The message acknowledgement mode.</param>
        /// <param name="messageQueue">The message queue to consume messages from.</param>
        public MessageConsumer(Session session,
            AcknowledgementMode acknowledgementMode, MessageQueue messageQueue)
            : this(session, acknowledgementMode, messageQueue, null)
        {
        }

        /// <summary>
        /// Constructs a message consumer on the specified queue, using a
        /// selector for filtering incoming messages.
        /// </summary>
        /// <param name="session">The messaging session.</param>
        /// <param name="acknowledgementMode">The message acknowledgement mode.</param>
        /// <param name="messageQueue">The message queue to consume messages from.</param>
        /// <param name="selector">The selection criteria.</param>
        public MessageConsumer(Session session,
            AcknowledgementMode acknowledgementMode, MessageQueue messageQueue,
            string selector)
        {
            this.session = session;
            this.acknowledgementMode = acknowledgementMode;
            this.messageQueue = messageQueue;
            if(this.messageQueue != null)
            {
                this.messageQueue.MessageReadPropertyFilter.SetAll();
            }

            reader = MessageReaderUtil.CreateMessageReader(
                messageQueue, session.MessageConverter, selector);
        }

        #region Asynchronous delivery

        private int listenerCount = 0;
        private event MessageListener listener;
        public event MessageListener Listener
        {
            add
            {
                listener += value;
                listenerCount++;

                session.Connection.ConnectionStateChange += OnConnectionStateChange;

                if(session.Connection.IsStarted)
                {
                    StartAsyncDelivery();
                }
            }

            remove
            {
                if(listenerCount > 0)
                {
                    listener -= value;
                    listenerCount--;
                }

                if(listenerCount == 0)
                {
                    session.Connection.ConnectionStateChange -= OnConnectionStateChange;

                    StopAsyncDelivery();
                }
            }
        }

        private void OnConnectionStateChange(object sender, Connection.StateChangeEventArgs e)
        {
            if(e.CurrentState == Connection.ConnectionState.Starting)
            {
                if(listenerCount > 0)
                {
                    StartAsyncDelivery();
                }
            }
            else if(e.CurrentState == Connection.ConnectionState.Stopping)
            {
                if(listenerCount > 0)
                {
                    StopAsyncDelivery();
                }
            }
        }

        private Thread asyncDeliveryThread = null;
        private Atomic<bool> asyncDelivery = new Atomic<bool>(false);
        TimeSpan dispatchingTimeout = new TimeSpan(5000);

        protected virtual void StartAsyncDelivery()
        {
            if(asyncDelivery.CompareAndSet(false, true))
            {
                asyncDeliveryThread = new Thread(new ThreadStart(DispatchLoop));
                asyncDeliveryThread.Name = "Message Consumer Dispatch: " + messageQueue.QueueName;
                asyncDeliveryThread.IsBackground = true;
                asyncDeliveryThread.Start();
            }
        }

        protected virtual void DispatchLoop()
        {
            Tracer.Info("Starting dispatcher thread consumer: " + this);
            while(asyncDelivery.Value)
            {
                try
                {
                    IMessage message = Receive(dispatchingTimeout);
                    if(asyncDelivery.Value && message != null)
                    {
                        try
                        {
                            listener(message);
                        }
                        catch(Exception e)
                        {
                            HandleAsyncException(e);
                        }
                    }
                }
                catch(ThreadAbortException ex)
                {
                    Tracer.InfoFormat("Thread abort received in thread: {0} : {1}", this, ex.Message);

                    break;
                }
                catch(Exception ex)
                {
                    Tracer.ErrorFormat("Exception while receiving message in thread: {0} : {1}", this, ex.Message);
                }
            }
            Tracer.Info("Stopping dispatcher thread consumer: " + this);
        }

        protected virtual void StopAsyncDelivery()
        {
            if(asyncDelivery.CompareAndSet(true, false))
            {
                if(null != asyncDeliveryThread)
                {
                    // Thread.Interrupt and Thread.Abort do not interrupt Receive
                    // instructions. Attempting to abort the thread and joining
                    // will result in a phantom backgroud thread, which may
                    // ultimately consume a message before actually stopping.

                    Tracer.Info("Waiting for thread to complete aborting.");
                    asyncDeliveryThread.Join(dispatchingTimeout);

                    asyncDeliveryThread = null;
                    Tracer.Info("Async delivery thread stopped.");
                }
            }
        }


        protected virtual void HandleAsyncException(Exception e)
        {
            session.Connection.HandleException(e);
        }

        #endregion

        #region Receive (synchronous)

        public IMessage Receive()
        {
            IMessage nmsMessage = null;

            if(messageQueue != null)
            {
                nmsMessage = reader.Receive();
                nmsMessage = TransformMessage(nmsMessage);
            }

            return nmsMessage;
        }

        public IMessage Receive(TimeSpan timeout)
        {
            IMessage nmsMessage = null;

            if(messageQueue != null)
            {
                try
                {
                    nmsMessage = reader.Receive(timeout);
                }
                catch(MessageQueueException ex)
                {
                    if(ex.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout)
                    {
                        throw ex;
                    }
                }
                nmsMessage = TransformMessage(nmsMessage);
            }

            return nmsMessage;
        }

        public IMessage ReceiveNoWait()
        {
            return Receive(zeroTimeout);
        }

        #endregion

        #region Close & dispose

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if(listenerCount > 0)
            {
                session.Connection.ConnectionStateChange -= OnConnectionStateChange;

                StopAsyncDelivery();
            }

            if(messageQueue != null)
            {
                messageQueue.Dispose();
                messageQueue = null;
            }
        }

        #endregion

        protected virtual IMessage TransformMessage(IMessage message)
        {
            IMessage transformed = message;

            if(message != null && this.ConsumerTransformer != null)
            {
                IMessage newMessage = ConsumerTransformer(this.session, this, message);
                if(newMessage != null)
                {
                    transformed = newMessage;
                }
            }

            return transformed;
        }
    }
}
