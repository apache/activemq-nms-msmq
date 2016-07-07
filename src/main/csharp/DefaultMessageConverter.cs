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

    /// <summary>
    /// This class provides default rules for converting MSMQ to and from
    /// NMS messages, when the peer system expects or produces compatible
    /// mappings, typically when the peer system is also implemented on
    /// Apache.NMS.
    /// Default mappings are as follows :
    /// <ul>
    /// <li>
    ///   the MSMQ Message.AppSetting field is used for specifying the NMS
    ///   message type, as specified by the <c>NMSMessageType</c> enumeration.
    /// </li>
    /// <li>
    ///   the MSMQ Message.Extension field is populated with a map
    ///   (a marshalled <c>PrimitiveMap</c>) of message properties.
    /// </li>
    /// <li>
    ///   in earlier versions of Apache.NMS.MSMQ, the MSMQ Message.Label
    ///   field was populated with the value of the NMSType field. Setting
    ///   <c>SetLabelAsNMSType</c> to true (the default value) applies that
    ///   same rule, which makes it compatible with existing NMS peers. If
    ///   set to false, the Message.Label field is populated with the value
    ///   of a "Label" property, if it exists, thus making it readable by
    ///   standard management or monitoring tools. The NMSType value is then
    ///   transmitted as a field in the Message.Extension map.
    /// </li>
    /// </ul>
    /// Please note that in earlier versions of Apache.NMS, only one property
    /// was set in the Message.Extension field : the NMSCorrelationID.
    /// The native Message.CorrelationId field is not settable, except for
    /// reply messages explicitely created as such through the MSMQ API.
    /// Transmission of the correlation id. through a mapped property called
    /// NMSCorrelationID is therefore maintained.
    /// When exchanging messages with a non compatible peer, a specific
    /// message converter must be provided, which should at least be able to
    /// map message types and define the encoding used for text messages.
    /// </summary>
	public class DefaultMessageConverter : IMessageConverterEx
	{
        private bool setLabelAsNMSType = true;
        public bool SetLabelAsNMSType
        {
            get { return setLabelAsNMSType; }
            set { setLabelAsNMSType = value; }
        }

        #region Messages
        /// <summary>
        /// Converts the specified NMS message to an equivalent MSMQ message.
        /// </summary>
        /// <param name="message">NMS message to be converted.</param>
        /// <result>Converted MSMQ message.</result>
		public virtual Message ToMsmqMessage(IMessage message)
		{
			Message msmqMessage = new Message();
			PrimitiveMap propertyData = new PrimitiveMap();

			ConvertMessageBodyToMSMQ(message, msmqMessage);

			if(message.NMSTimeToLive != TimeSpan.Zero)
			{
				msmqMessage.TimeToBeReceived = message.NMSTimeToLive;
			}

			if(message.NMSCorrelationID != null)
			{
				propertyData.SetString("NMSCorrelationID", message.NMSCorrelationID);
			}

			msmqMessage.Recoverable = (message.NMSDeliveryMode == MsgDeliveryMode.Persistent);
			msmqMessage.Priority = ToMsmqMessagePriority(message.NMSPriority);
			msmqMessage.ResponseQueue = ToMsmqDestination(message.NMSReplyTo);
			if(message.NMSType != null)
			{
                if(SetLabelAsNMSType)
                {
				    propertyData.SetString("NMSType", message.NMSType);
                }
                else
                {
                    msmqMessage.Label = message.NMSType;
                }
			}

            // Populate property data
            foreach(object keyObject in message.Properties.Keys)
            {
              string key = (keyObject as string);
              object val = message.Properties.GetString(key);
              if(!SetLabelAsNMSType && string.Compare(key, "Label", true) == 0 && val != null)
              {
				msmqMessage.Label = val.ToString();
              }
              else
              {
				propertyData[key] = val;
              }
            }

			// Store the NMS property data in the extension area
			msmqMessage.Extension = propertyData.Marshal();
			return msmqMessage;
		}

        /// <summary>
        /// Converts the specified MSMQ message to an equivalent NMS message
        /// (including its message body).
        /// </summary>
        /// <param name="message">MSMQ message to be converted.</param>
        /// <result>Converted NMS message.</result>
		public virtual IMessage ToNmsMessage(Message message)
		{
            return ToNmsMessage(message, true);
        }

        /// <summary>
        /// Converts the specified MSMQ message to an equivalent NMS message.
        /// </summary>
        /// <param name="message">MSMQ message to be converted.</param>
        /// <param name="convertBody">true if message body should be converted.</param>
        /// <result>Converted NMS message.</result>
		public virtual IMessage ToNmsMessage(Message message, bool convertBody)
		{
			BaseMessage answer = CreateNmsMessage(message, convertBody);

			// Get the NMS property data from the extension area
			PrimitiveMap propertyData = PrimitiveMap.Unmarshal(message.Extension);

			try
			{
				answer.NMSMessageId = message.Id;
				answer.NMSCorrelationID = propertyData.GetString("NMSCorrelationID");
				answer.NMSDeliveryMode = (message.Recoverable ? MsgDeliveryMode.Persistent : MsgDeliveryMode.NonPersistent);
				answer.NMSDestination = ToNmsDestination(message.DestinationQueue);
			}
			catch(InvalidOperationException)
			{
			}

			try
			{
				answer.NMSReplyTo = ToNmsDestination(message.ResponseQueue);
				answer.NMSTimeToLive = message.TimeToBeReceived;
			    answer.NMSPriority = ToNmsMsgPriority(message.Priority);
			}
			catch(InvalidOperationException)
			{
			}

			try
			{
                if(message.Label != null)
                {
                    if(SetLabelAsNMSType)
                    {
                        answer.NMSType = message.Label;
                    }
                    else
                    {
                        answer.Properties["Label"] = message.Label;
                    }
                }
                answer.Properties["LookupId"] = message.LookupId;
			}
			catch(InvalidOperationException)
			{
			}

            foreach(object keyObject in propertyData.Keys)
            {
			    try
			    {
                    string key = (keyObject as string);
                    if(string.Compare(key, "NMSType", true) == 0)
                    {
			    	    answer.NMSType = propertyData.GetString(key);
                    }
                    else if(string.Compare(key, "NMSCorrelationID", true) == 0)
                    {
			    	    answer.NMSCorrelationID = propertyData.GetString("NMSCorrelationID");
                    }
                    else
                    {
			    	    answer.Properties[key] = propertyData[key];
                    }
			    }
			    catch(InvalidOperationException)
			    {
			    }
            }
			return answer;
		}

        #endregion

        #region Message priority

        // Message priorities are defined as follows :
        // | MSMQ               | NMS                |
        // | MessagePriority	| MsgPriority        |
        // +--------------------+--------------------+
        // | Lowest             | Lowest             |
        // | VeryLow            | VeryLow            |
        // | Low                | Low                |
        // |                \-> | AboveLow           |
        // |                /-> | BelowNormal        |
        // | Normal             | Normal             |
        // | AboveNormal        | AboveNormal        |
        // | High               | High               |
        // | VeryHigh           | VeryHigh           |
        // | Highest            | Highest            |
        // +--------------------+--------------------+

        /// <summary>
        /// Converts the specified NMS message priority to an equivalent MSMQ
        /// message priority.
        /// </summary>
        /// <param name="msgPriority">NMS message priority to be converted.</param>
        /// <result>Converted MSMQ message priority.</result>
		private static MessagePriority ToMsmqMessagePriority(MsgPriority msgPriority)
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

        /// <summary>
        /// Converts the specified MSMQ message priority to an equivalent NMS
        /// message priority.
        /// </summary>
        /// <param name="messagePriority">MSMQ message priority to be converted.</param>
        /// <result>Converted NMS message priority.</result>
		private static MsgPriority ToNmsMsgPriority(MessagePriority messagePriority)
		{
			switch(messagePriority)
			{
			case MessagePriority.Lowest:
				return MsgPriority.Lowest;

			case MessagePriority.VeryLow:
				return MsgPriority.VeryLow;

			case MessagePriority.Low:
				return MsgPriority.Low;

			default:
			case MessagePriority.Normal:
				return MsgPriority.Normal;

			case MessagePriority.AboveNormal:
				return MsgPriority.AboveNormal;

			case MessagePriority.High:
				return MsgPriority.High;

			case MessagePriority.VeryHigh:
				return MsgPriority.VeryHigh;

			case MessagePriority.Highest:
				return MsgPriority.Highest;
			}
		}

        #endregion

        #region Message creation

        // Conversion of the message body has been separated from the creation
        // of the NMS message object for performance reasons when using
        // selectors (selectors handle only message attributes, not message
        // bodies).
        // CreateNmsMessage(Message) is maintained for compatibility reasons
        // with existing clients that may have implemented derived classes,
        // instead of completely removing the body conversion part from the
        // method.

        /// <summary>
        /// Creates an NMS message of appropriate type for the specified MSMQ
        /// message, and convert the message body.
        /// </summary>
        /// <param name="message">MSMQ message.</param>
        /// <result>NMS message created for retrieving the MSMQ message.</result>
		protected virtual BaseMessage CreateNmsMessage(Message message)
		{
            return CreateNmsMessage(message, true);
        }

        /// <summary>
        /// Creates an NMS message of appropriate type for the specified MSMQ
        /// message, and convert the message body if specified.
        /// </summary>
        /// <param name="message">MSMQ message.</param>
        /// <param name="convertBody">true if the message body must be
        /// converted.</param>
        /// <result>NMS message created for retrieving the MSMQ message.</result>
		protected virtual BaseMessage CreateNmsMessage(Message message,
            bool convertBody)
		{
			BaseMessage result = null;

			if((int) NMSMessageType.TextMessage == message.AppSpecific)
			{
				TextMessage textMessage = new TextMessage();

                if(convertBody)
                {
                    ConvertTextMessageBodyToNMS(message, textMessage);
                }

				result = textMessage;
			}
			else if((int) NMSMessageType.BytesMessage == message.AppSpecific)
			{
				BytesMessage bytesMessage = new BytesMessage();

                if(convertBody)
                {
                    ConvertBytesMessageBodyToNMS(message, bytesMessage);
                }

				result = bytesMessage;
			}
			else if((int) NMSMessageType.ObjectMessage == message.AppSpecific)
			{
				ObjectMessage objectMessage = new ObjectMessage();

                if(convertBody)
                {
                    ConvertObjectMessageBodyToNMS(message, objectMessage);
                }

				result = objectMessage;
			}
			else if((int) NMSMessageType.MapMessage == message.AppSpecific)
			{
				MapMessage mapMessage = new MapMessage();

                if(convertBody)
                {
                    ConvertMapMessageBodyToNMS(message, mapMessage);
                }

				result = mapMessage;
			}
			else if((int) NMSMessageType.StreamMessage == message.AppSpecific)
			{
				StreamMessage streamMessage = new StreamMessage();

                if(convertBody)
                {
                    ConvertStreamMessageBodyToNMS(message, streamMessage);
                }

				result = streamMessage;
			}
			else
			{
				BaseMessage baseMessage = new BaseMessage();
				result = baseMessage;
			}

			return result;
		}

        #endregion

        #region Message body

        /// <summary>
        /// Converts an NMS message body to the equivalent MSMQ message body.
        /// </summary>
        /// <param name="message">Source NMS message.</param>
        /// <param name="answer">Target MSMQ message.</param>
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

        /// <summary>
        /// Converts an MSMQ message body to the equivalent NMS message body.
        /// </summary>
        /// <param name="message">Source MSMQ message.</param>
        /// <param name="answer">Target NMS message.</param>
		public virtual void ConvertMessageBodyToNMS(Message message, IMessage answer)
		{
			if(answer is TextMessage)
			{
				ConvertTextMessageBodyToNMS(message, (TextMessage)answer);
			}
			else if(answer is BytesMessage)
			{
				ConvertBytesMessageBodyToNMS(message, (BytesMessage)answer);
			}
			else if(answer is ObjectMessage)
			{
				ConvertObjectMessageBodyToNMS(message, (ObjectMessage)answer);
			}
			else if(answer is MapMessage)
			{
				ConvertMapMessageBodyToNMS(message, (MapMessage)answer);
			}
			else if(answer is StreamMessage)
			{
				ConvertStreamMessageBodyToNMS(message, (StreamMessage)answer);
			}

			return;
		}

        /// <summary>
        /// Converts an MSMQ message body to the equivalent NMS text message
        /// body.
        /// </summary>
        /// <param name="message">Source MSMQ message.</param>
        /// <param name="answer">Target NMS text message.</param>
		public virtual void ConvertTextMessageBodyToNMS(Message message,
            TextMessage answer)
		{
			string content = String.Empty;

			if(message.BodyStream != null && message.BodyStream.Length > 0)
			{
				byte[] buf = new byte[message.BodyStream.Length];
				message.BodyStream.Read(buf, 0, buf.Length);
				content = Encoding.UTF32.GetString(buf);
			}

			answer.Text = content;
		}

        /// <summary>
        /// Converts an MSMQ message body to the equivalent NMS bytes message
        /// body.
        /// </summary>
        /// <param name="message">Source MSMQ message.</param>
        /// <param name="answer">Target NMS bytes message.</param>
		public virtual void ConvertBytesMessageBodyToNMS(Message message,
            BytesMessage answer)
		{
			byte[] buf = null;

			if(message.BodyStream != null && message.BodyStream.Length > 0)
			{
				buf = new byte[message.BodyStream.Length];
				message.BodyStream.Read(buf, 0, buf.Length);
			}

			answer.Content = buf;
		}

        /// <summary>
        /// Converts an MSMQ message body to the equivalent NMS object message
        /// body.
        /// </summary>
        /// <param name="message">Source MSMQ message.</param>
        /// <param name="answer">Target NMS object message.</param>
		public virtual void ConvertObjectMessageBodyToNMS(Message message,
            ObjectMessage answer)
		{
			answer.Body = message.Body;
		}

        /// <summary>
        /// Converts an MSMQ message body to the equivalent NMS map message
        /// body.
        /// </summary>
        /// <param name="message">Source MSMQ message.</param>
        /// <param name="answer">Target NMS map message.</param>
		public virtual void ConvertMapMessageBodyToNMS(Message message,
            MapMessage answer)
		{
			byte[] buf = null;

			if(message.BodyStream != null && message.BodyStream.Length > 0)
			{
				buf = new byte[message.BodyStream.Length];
				message.BodyStream.Read(buf, 0, buf.Length);
			}

			answer.Body = PrimitiveMap.Unmarshal(buf);
		}

        /// <summary>
        /// Converts an MSMQ message body to the equivalent NMS stream message
        /// body.
        /// </summary>
        /// <param name="message">Source MSMQ message.</param>
        /// <param name="answer">Target NMS stream message.</param>
		public virtual void ConvertStreamMessageBodyToNMS(Message message,
            StreamMessage answer)
		{
			// TODO: Implement
            throw new NotImplementedException();
		}

        #endregion

        #region Destination

        /// <summary>
        /// Converts an NMS destination to the equivalent MSMQ destination
        /// (ie. queue).
        /// </summary>
        /// <param name="destination">NMS destination.</param>
        /// <result>MSMQ queue.</result>
		public MessageQueue ToMsmqDestination(IDestination destination)
		{
			if(null == destination)
			{
				return null;
			}

			return new MessageQueue((destination as Destination).Path);
		}

        /// <summary>
        /// Converts an MSMQ destination (ie. queue) to the equivalent NMS
        /// destination.
        /// </summary>
        /// <param name="destinationQueue">MSMQ destination queue.</param>
        /// <result>NMS destination.</result>
		protected virtual IDestination ToNmsDestination(MessageQueue destinationQueue)
		{
			if(null == destinationQueue)
			{
				return null;
			}

			return new Queue(destinationQueue.Path);
		}

        #endregion
	}
}
