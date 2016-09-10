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

using System;
using System.Threading;

namespace Apache.NMS.MSMQ
{
    /// <summary>
    /// Represents a NMS connection MSMQ.  Since the underlying MSMQ APIs are actually
    /// connectionless, NMS connection in the MSMQ case are not expensive operations.
    /// </summary>
    ///
    public class Connection : IConnection
    {
        #region Constructors

        public Connection()
        {
            // now lets send the connection and see if we get an ack/nak
            // TODO: establish a connection
        }

        #endregion

        #region Connection state

        public enum ConnectionState
        {
            Created,
            Connected,
            Starting,
            Started,
            Stopping,
            Stopped,
            Closed
        }

        private ConnectionState state = ConnectionState.Created;

        public class StateChangeEventArgs : EventArgs
        {
            public StateChangeEventArgs(ConnectionState originalState,
                ConnectionState currentState)
            {
                this.originalState = originalState;
                this.currentState = currentState;
            }

            private ConnectionState originalState;
            public ConnectionState OriginalState
            {
                get { return originalState; }
            }

            private ConnectionState currentState;
            public ConnectionState CurrentState
            {
                get { return currentState; }
            }
        }

        public delegate void StateChangeEventHandler(object sender, StateChangeEventArgs e);

        public event StateChangeEventHandler ConnectionStateChange;

        private void ChangeState(ConnectionState newState)
        {
            if(ConnectionStateChange != null)
            {
                ConnectionStateChange(this, 
                    new StateChangeEventArgs(this.state, newState));
            }

            this.state = newState;
        }

        private object stateLock = new object();

        #endregion

        #region Start & stop

        /// <summary>
        /// Starts message delivery for this connection.
        /// </summary>
        public void Start()
        {
            lock(stateLock)
            {
                switch(state)
                {
                    case ConnectionState.Created:
                    case ConnectionState.Connected:
                    case ConnectionState.Stopped:
                        ChangeState(ConnectionState.Starting);
                        ChangeState(ConnectionState.Started);
                        break;

                    case ConnectionState.Stopping:
                        throw new NMSException("Connection stopping");

                    case ConnectionState.Closed:
                        throw new NMSException("Connection closed");

                    case ConnectionState.Starting:
                    case ConnectionState.Started:
                        break;
                }
            }
        }

        /// <summary>
        /// This property determines if the asynchronous message delivery of incoming
        /// messages has been started for this connection.
        /// </summary>
        public bool IsStarted
        {
            get { return state == ConnectionState.Started; }
        }

        /// <summary>
        /// Stop message delivery for this connection.
        /// </summary>
        public void Stop()
        {
            lock(stateLock)
            {
                switch(state)
                {
                    case ConnectionState.Started:
                        ChangeState(ConnectionState.Stopping);
                        ChangeState(ConnectionState.Stopped);
                        break;

                    case ConnectionState.Starting:
                        throw new NMSException("Connection starting");

                    case ConnectionState.Closed:
                        throw new NMSException("Connection closed");

                    case ConnectionState.Created:
                    case ConnectionState.Connected:
                    case ConnectionState.Stopping:
                    case ConnectionState.Stopped:
                        break;
                }
            }
        }

        #endregion

        #region Close & dispose

        public void Close()
        {
            if(!IsClosed)
            {
                Stop();

                state = ConnectionState.Closed;
            }
        }

        public bool IsClosed
        {
            get { return state == ConnectionState.Closed; }
        }

        public void Dispose()
        {
            try
            {
                Close();
            }
            catch
            {
                state = ConnectionState.Closed;
            }
        }

        #endregion

        #region Create session

        /// <summary>
        /// Creates a new session to work on this connection
        /// </summary>
        public ISession CreateSession()
        {
            return CreateSession(acknowledgementMode);
        }

        /// <summary>
        /// Creates a new session to work on this connection
        /// </summary>
        public ISession CreateSession(AcknowledgementMode mode)
        {
            if(IsClosed)
            {
                throw new NMSException("Connection closed");
            }
            return new Session(this, mode);
        }

        #endregion

        #region Connection properties

        /// <summary>
        /// The default timeout for network requests.
        /// </summary>
        public TimeSpan RequestTimeout
        {
            get { return NMSConstants.defaultRequestTimeout; }
            set { }
        }

        private AcknowledgementMode acknowledgementMode = AcknowledgementMode.AutoAcknowledge;
        public AcknowledgementMode AcknowledgementMode
        {
            get { return acknowledgementMode; }
            set { acknowledgementMode = value; }
        }

        private IMessageConverter messageConverter = new DefaultMessageConverter();
        public IMessageConverter MessageConverter
        {
            get { return messageConverter; }
            set { messageConverter = value; }
        }

        private string clientId;
        public string ClientId
        {
            get { return clientId; }
            set
            {
                if(state != ConnectionState.Created)
                {
                    throw new NMSException("You cannot change the ClientId once the Connection is connected");
                }
                clientId = value;
            }
        }

        private IRedeliveryPolicy redeliveryPolicy;
        /// <summary>
        /// Get/or set the redelivery policy for this connection.
        /// </summary>
        public IRedeliveryPolicy RedeliveryPolicy
        {
            get { return this.redeliveryPolicy; }
            set { this.redeliveryPolicy = value; }
        }

        private ConnectionMetaData metaData = null;
        /// <summary>
        /// Gets the Meta Data for the NMS Connection instance.
        /// </summary>
        public IConnectionMetaData MetaData
        {
            get { return this.metaData ?? (this.metaData = new ConnectionMetaData()); }
        }

        #endregion

        #region Transformer delegates

        private ConsumerTransformerDelegate consumerTransformer;
        public ConsumerTransformerDelegate ConsumerTransformer
        {
            get { return this.consumerTransformer; }
            set { this.consumerTransformer = value; }
        }

        private ProducerTransformerDelegate producerTransformer;
        public ProducerTransformerDelegate ProducerTransformer
        {
            get { return this.producerTransformer; }
            set { this.producerTransformer = value; }
        }

        #endregion

        #region Exception & transport listeners

        /// <summary>
        /// A delegate that can receive transport level exceptions.
        /// </summary>
        public event ExceptionListener ExceptionListener;

        public void HandleException(Exception e)
        {
            if(ExceptionListener != null && !this.IsClosed)
            {
                ExceptionListener(e);
            }
            else
            {
                Tracer.Error(e);
            }
        }

        /// <summary>
        /// An asynchronous listener that is notified when a Fault tolerant connection
        /// has been interrupted.
        /// </summary>
        public event ConnectionInterruptedListener ConnectionInterruptedListener;

        public void HandleTransportInterrupted()
        {
            Tracer.Debug("Transport has been Interrupted.");

            if(this.ConnectionInterruptedListener != null && !this.IsClosed)
            {
                try
                {
                    this.ConnectionInterruptedListener();
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// An asynchronous listener that is notified when a Fault tolerant connection
        /// has been resumed.
        /// </summary>
        public event ConnectionResumedListener ConnectionResumedListener;

        public void HandleTransportResumed()
        {
            Tracer.Debug("Transport has resumed normal operation.");

            if(this.ConnectionResumedListener != null && !this.IsClosed)
            {
                try
                {
                    this.ConnectionResumedListener();
                }
                catch
                {
                }
            }
        }

        #endregion

        public void PurgeTempDestinations()
        {
        }
    }
}
