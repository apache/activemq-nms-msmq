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
using System.Messaging;

namespace Apache.NMS.MSMQ
{

	/// <summary>
	/// Summary description for Queue.
	/// </summary>
	public class Queue : Destination, IQueue
	{
		public Queue()
			: base()
		{
		}

		public Queue(MessageQueue messageQueue)
			: base()
		{
			this.messageQueue = messageQueue;
            Path = messageQueue.Path;
		}

		public Queue(String name)
			: base(name)
		{
			if(string.IsNullOrEmpty(name))
			{
				messageQueue = null;
			}
            else
            {
                try
                {
                    messageQueue = new MessageQueue(name);
                }
                catch(Exception /*ex*/)
                {
                    // Excerpt from Microsoft documentation for MessageQueue.Exists :
                    // (@https://msdn.microsoft.com/fr-fr/library/system.messaging.messagequeue.exists(v=vs.110).aspx)
                    // Exists(String) is an expensive operation. Use it only when it is necessary within the application.
                    // ---
                    // Therefore, we won't check for existence of the queue before attempting to access it.

                    //if(!Exists(name))
                    //{
                        // Excerpt from the Oracle JMS JavaDoc for Session.createQueue :
                        // (@https://docs.oracle.com/javaee/7/api/javax/jms/Session.html#createQueue-java.lang.String-)
                        // Note that this method simply creates an object that encapsulates the name of a queue. It does
                        // not create the physical queue in the JMS provider. JMS does not provide a method to create the
                        // physical queue, since this would be specific to a given JMS provider. Creating a physical queue
                        // is provider-specific and is typically an administrative task performed by an administrator,
                        // though some providers may create them automatically when needed. The one exception to this is
                        // the creation of a temporary queue, which is done using the createTemporaryQueue method.
                        // ---
                        // Therefore, we should throw an NMSException if the queue does not exist.
                        // ---
                        // BUT, to keep it compatible with the initial implementation of MessageProducer, which attempts
                        // to create non pre-existing queues, we keep it silent..

                        // throw new NMSException("Message queue \"" + name + "\" does not exist", ex);
                    //}

                    // throw new NMSException("Cannot access message queue \"" + name + "\"", ex);
                }
            }
		}

        private MessageQueue messageQueue;
        public MessageQueue MSMQMessageQueue
        {
            get { return messageQueue; }
        }

		override public DestinationType DestinationType
		{
			get
			{
				return DestinationType.Queue;
			}
		}

		public String QueueName
		{
			get { return Path; }
		}


		public override Destination CreateDestination(String name)
		{
			return new Queue(name);
		}

        public static bool Exists(string name)
        {
            try
            {
                return MessageQueue.Exists(name);
            }
            catch(InvalidOperationException)
            {
                // Excerpt from Microsoft documentation for MessageQueue.Exists :
                // (@https://msdn.microsoft.com/fr-fr/library/system.messaging.messagequeue.exists(v=vs.110).aspx)
                // InvalidOperationException: The application used format name syntax when verifying queue existence. 
                // ---
                // The Exists(String) method does not support the FormatName prefix.
                // No method exists to determine whether a queue with a specified format name exists.

                // We'll assume the queue exists at this point
                return true;
            }
        }
	}
}

