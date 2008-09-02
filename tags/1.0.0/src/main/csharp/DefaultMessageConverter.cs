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
using System.Text;
using System.Messaging;
using System.IO;
using Apache.NMS;

namespace Apache.NMS.MSMQ
{
    public class DefaultMessageConverter : IMessageConverter
	{
        public virtual Message ToMsmqMessage(IMessage message)
        {
            Message answer = new Message();
            ConvertMessageBodyToMSMQ(message, answer);
            MessageQueue responseQueue=null;
            if (message.NMSReplyTo != null)
            {
                IDestination destination = message.NMSReplyTo;
				responseQueue = ToMsmqDestination(destination);
            }
            if (message.NMSTimeToLive != TimeSpan.Zero)
            {
                answer.TimeToBeReceived = message.NMSTimeToLive;
            }
            if (message.NMSCorrelationID != null)
            {
                answer.CorrelationId = message.NMSCorrelationID;
            }
            answer.Recoverable = message.NMSPersistent;
            answer.Priority = MessagePriority.Normal;
            answer.ResponseQueue = responseQueue;
			if(message.NMSType != null)
			{
				answer.Label = message.NMSType;
			}
            return answer;
        }
		
        protected virtual void ConvertMessageBodyToMSMQ(IMessage message,
                                                        Message answer)
        {
            if (message is IBytesMessage)
            {
                byte[] bytes = (message as IBytesMessage).Content;
                answer.BodyStream.Write(bytes, 0, bytes.Length);
            }
            else
            {
                throw new Exception("unhandled message type");
            }
        }

        public virtual IMessage ToNmsMessage(Message message)
        {
			BaseMessage answer = CreateNmsMessage(message);
			answer.NMSMessageId = message.Id;
			try
			{
				answer.NMSCorrelationID = message.CorrelationId;
			}
			catch (InvalidOperationException)
			{
			}

			try
			{
				answer.NMSDestination = ToNmsDestination(message.DestinationQueue);
			}
			catch (InvalidOperationException)
			{
			}

			answer.NMSType = message.Label;
			answer.NMSReplyTo = ToNmsDestination(message.ResponseQueue);
			try
			{
				answer.NMSTimeToLive = message.TimeToBeReceived;
			}
			catch (InvalidOperationException)
			{
			}
            return answer;
        }
		
		
		public MessageQueue ToMsmqDestination(IDestination destination)
		{
			return new MessageQueue((destination as Destination).Path);
		}

		protected virtual IDestination ToNmsDestination(MessageQueue destinationQueue)
		{
			if (destinationQueue == null)
			{
				return null;
			}
			return new Queue(destinationQueue.Path);
		}
	
		protected virtual BaseMessage CreateNmsMessage(Message message)
		{
			Stream stream = message.BodyStream;
			if (stream == null || stream.Length == 0)
			{
				return new BaseMessage();
			}
			byte[] buf = new byte[stream.Length];
			stream.Read(buf, 0, buf.Length);
			// TODO: how to recognise other flavors of message?
			BytesMessage result =  new BytesMessage();
			result.Content = buf;
			return result;
		}
	}
}
