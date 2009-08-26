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
using Apache.NMS.Util;

namespace Apache.NMS.MSMQ
{
	public enum NMSMessageType
	{
		BaseMessage,
		TextMessage,
		BytesMessage,
		ObjectMessage,
		MapMessage,
		StreamMessage
	}

	public class DefaultMessageConverter : IMessageConverter
	{
		public virtual Message ToMsmqMessage(IMessage message)
		{
			Message msmqMessage = new Message();
			PrimitiveMap metaData = new PrimitiveMap();

			ConvertMessageBodyToMSMQ(message, msmqMessage);

			if(message.NMSTimeToLive != TimeSpan.Zero)
			{
				msmqMessage.TimeToBeReceived = message.NMSTimeToLive;
			}

			if(message.NMSCorrelationID != null)
			{
				metaData.SetString("NMSCorrelationID", message.NMSCorrelationID);
			}

			msmqMessage.Recoverable = (message.NMSDeliveryMode == MsgDeliveryMode.Persistent);
			msmqMessage.Priority = ToMessagePriority(message.NMSPriority);
			msmqMessage.ResponseQueue = ToMsmqDestination(message.NMSReplyTo);
			if(message.NMSType != null)
			{
				msmqMessage.Label = message.NMSType;
			}

			// Store the NMS meta data in the extension area
			msmqMessage.Extension = metaData.Marshal();
			return msmqMessage;
		}

		public virtual IMessage ToNmsMessage(Message message)
		{
			BaseMessage answer = CreateNmsMessage(message);
			// Get the NMS meta data from the extension area
			PrimitiveMap metaData = PrimitiveMap.Unmarshal(message.Extension);

			try
			{
				answer.NMSMessageId = message.Id;
				answer.NMSCorrelationID = metaData.GetString("NMSCorrelationID");
				answer.NMSDeliveryMode = (message.Recoverable ? MsgDeliveryMode.Persistent : MsgDeliveryMode.NonPersistent);
				answer.NMSDestination = ToNmsDestination(message.DestinationQueue);
			}
			catch(InvalidOperationException)
			{
			}

			try
			{
				answer.NMSType = message.Label;
				answer.NMSReplyTo = ToNmsDestination(message.ResponseQueue);
				answer.NMSTimeToLive = message.TimeToBeReceived;
			}
			catch(InvalidOperationException)
			{
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

		protected virtual void ConvertMessageBodyToMSMQ(IMessage message, Message answer)
		{
			if(message is TextMessage)
			{
				TextMessage textMessage = message as TextMessage;
				byte[] buf = Encoding.UTF32.GetBytes(textMessage.Text);
				answer.BodyStream.Write(buf, 0, buf.Length);
				answer.AppSpecific = (int) NMSMessageType.TextMessage;
			}
			else if(message is BytesMessage)
			{
				BytesMessage bytesMessage = message as BytesMessage;
				answer.BodyStream.Write(bytesMessage.Content, 0, bytesMessage.Content.Length);
				answer.AppSpecific = (int) NMSMessageType.BytesMessage;
			}
			else if(message is ObjectMessage)
			{
				ObjectMessage objectMessage = message as ObjectMessage;
				answer.Body = objectMessage.Body;
				answer.AppSpecific = (int) NMSMessageType.ObjectMessage;
			}
			else if(message is MapMessage)
			{
				MapMessage mapMessage = message as MapMessage;
				PrimitiveMap mapBody = mapMessage.Body as PrimitiveMap;
				byte[] buf = mapBody.Marshal();
				answer.BodyStream.Write(buf, 0, buf.Length);
				answer.AppSpecific = (int) NMSMessageType.MapMessage;
			}
			else if(message is StreamMessage)
			{
				StreamMessage streamMessage = message as StreamMessage;
				answer.AppSpecific = (int) NMSMessageType.StreamMessage;
				// TODO: Implement
			}
			else if(message is BaseMessage)
			{
				answer.AppSpecific = (int) NMSMessageType.BaseMessage;
			}
			else
			{
				throw new Exception("unhandled message type");
			}
		}

		protected virtual BaseMessage CreateNmsMessage(Message message)
		{
			BaseMessage result = null;

			if((int) NMSMessageType.TextMessage == message.AppSpecific)
			{
				TextMessage textMessage = new TextMessage();
				string content = String.Empty;

				if(message.BodyStream != null && message.BodyStream.Length > 0)
				{
					byte[] buf = null;
					buf = new byte[message.BodyStream.Length];
					message.BodyStream.Read(buf, 0, buf.Length);
					content = Encoding.UTF32.GetString(buf);
				}

				textMessage.Text = content;
				result = textMessage;
			}
			else if((int) NMSMessageType.BytesMessage == message.AppSpecific)
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
			else if((int) NMSMessageType.ObjectMessage == message.AppSpecific)
			{
				ObjectMessage objectMessage = new ObjectMessage();

				objectMessage.Body = message.Body;
				result = objectMessage;
			}
			else if((int) NMSMessageType.MapMessage == message.AppSpecific)
			{
				byte[] buf = null;

				if(message.BodyStream != null && message.BodyStream.Length > 0)
				{
					buf = new byte[message.BodyStream.Length];
					message.BodyStream.Read(buf, 0, buf.Length);
				}

				MapMessage mapMessage = new MapMessage();
				mapMessage.Body = PrimitiveMap.Unmarshal(buf);
				result = mapMessage;
			}
			else if((int) NMSMessageType.StreamMessage == message.AppSpecific)
			{
				StreamMessage streamMessage = new StreamMessage();

				// TODO: Implement
				result = streamMessage;
			}
			else
			{
				BaseMessage baseMessage = new BaseMessage();

				result = baseMessage;
			}

			return result;
		}

		public MessageQueue ToMsmqDestination(IDestination destination)
		{
			if(null == destination)
			{
				return null;
			}

			return new MessageQueue((destination as Destination).Path);
		}

		protected virtual IDestination ToNmsDestination(MessageQueue destinationQueue)
		{
			if(null == destinationQueue)
			{
				return null;
			}

			return new Queue(destinationQueue.Path);
		}
	}
}
