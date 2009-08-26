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
using System.IO;
using System.Messaging;
using System.Text;

namespace Apache.NMS.MSMQ
{
	public enum NMSMessageType
	{
		BytesMessage,
		TextMessage,
		MapMessage
	}

	public class DefaultMessageConverter : IMessageConverter
	{
		public virtual Message ToMsmqMessage(IMessage message)
		{
			Message answer = new Message();
			ConvertMessageBodyToMSMQ(message, answer);
			MessageQueue responseQueue = null;
			if(message.NMSReplyTo != null)
			{
				IDestination destination = message.NMSReplyTo;
				responseQueue = ToMsmqDestination(destination);
			}

			if(message.NMSTimeToLive != TimeSpan.Zero)
			{
				answer.TimeToBeReceived = message.NMSTimeToLive;
			}

			if(message.NMSCorrelationID != null)
			{
				answer.CorrelationId = message.NMSCorrelationID;
			}

			answer.Recoverable = (message.NMSDeliveryMode == MsgDeliveryMode.Persistent);
			answer.Priority = ToMessagePriority(message.NMSPriority);
			answer.ResponseQueue = responseQueue;
			if(message.NMSType != null)
			{
				answer.Label = message.NMSType;
			}
			
			return answer;
		}

		private static MessagePriority ToMessagePriority(MsgPriority msgPriority)
		{
			switch(msgPriority)
			{
			case MsgPriority.Lowest:
				return MessagePriority.Lowest;

			case MsgPriority.VeryLow:
				return MessagePriority.VeryLow;

			case MsgPriority.Low:
			case MsgPriority.AboveLow:
				return MessagePriority.Low;

			default:
			case MsgPriority.BelowNormal:
			case MsgPriority.Normal:
				return MessagePriority.Normal;

			case MsgPriority.AboveNormal:
				return MessagePriority.AboveNormal;

			case MsgPriority.High:
				return MessagePriority.High;

			case MsgPriority.VeryHigh:
				return MessagePriority.VeryHigh;

			case MsgPriority.Highest:
				return MessagePriority.Highest;
			}
		}

		protected virtual void ConvertMessageBodyToMSMQ(IMessage message,
														Message answer)
		{
			if(message is IBytesMessage)
			{
				IBytesMessage bytesMessage = message as IBytesMessage;
				answer.BodyStream.Write(bytesMessage.Content, 0, bytesMessage.Content.Length);
				answer.AppSpecific = (int) NMSMessageType.BytesMessage;
			}
			else if(message is ITextMessage)
			{
				ITextMessage textMessage = message as ITextMessage;
				byte[] buf = Encoding.UTF8.GetBytes(textMessage.Text);
				answer.BodyStream.Write(buf, 0, buf.Length);
				answer.AppSpecific = (int) NMSMessageType.TextMessage;
			}
			else if(message is IMapMessage)
			{
				IMapMessage mapMessage = message as IMapMessage;
				answer.Body = mapMessage.Body;
				answer.AppSpecific = (int) NMSMessageType.MapMessage;
			}
			else
			{
				throw new Exception("unhandled message type");
			}
		}

		protected virtual BaseMessage CreateNmsMessage(Message message)
		{
			BaseMessage result = null;

			if((int) NMSMessageType.BytesMessage == message.AppSpecific)
			{
				byte[] buf = null;

				if(message.BodyStream != null && message.BodyStream.Length > 0)
				{
					buf = new byte[message.BodyStream.Length];
					message.BodyStream.Read(buf, 0, buf.Length);
				}

				BytesMessage bytesMessage = new BytesMessage();
				bytesMessage.Content = buf;
				result = bytesMessage;
			}
			else if((int) NMSMessageType.TextMessage == message.AppSpecific)
			{
				TextMessage textMessage = new TextMessage();
				string content = String.Empty;

				if(message.BodyStream != null && message.BodyStream.Length > 0)
				{
					byte[] buf = null;
					buf = new byte[message.BodyStream.Length];
					message.BodyStream.Read(buf, 0, buf.Length);
					content = Encoding.UTF8.GetString(buf);
				}

				textMessage.Text = content;
				result = textMessage;
			}
			else if((int) NMSMessageType.MapMessage == message.AppSpecific)
			{
				MapMessage mapMessage = new MapMessage();

				mapMessage.Body = message.Body as IPrimitiveMap;
				result = mapMessage;
			}
			else
			{
				result = new BaseMessage();
			}

			return result;
		}

		public virtual IMessage ToNmsMessage(Message message)
		{
			BaseMessage answer = CreateNmsMessage(message);
			answer.NMSMessageId = message.Id;
			try
			{
				answer.NMSCorrelationID = message.CorrelationId;
				answer.NMSDeliveryMode = (message.Recoverable ? MsgDeliveryMode.Persistent : MsgDeliveryMode.NonPersistent);
			}
			catch(InvalidOperationException)
			{
			}

			try
			{
				answer.NMSDestination = ToNmsDestination(message.DestinationQueue);
			}
			catch(InvalidOperationException)
			{
			}

			answer.NMSType = message.Label;
			answer.NMSReplyTo = ToNmsDestination(message.ResponseQueue);
			try
			{
				answer.NMSTimeToLive = message.TimeToBeReceived;
			}
			catch(InvalidOperationException)
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
			if(destinationQueue == null)
			{
				return null;
			}
			return new Queue(destinationQueue.Path);
		}
	}
}
