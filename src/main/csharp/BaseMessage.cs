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
using Apache.NMS.Util;

namespace Apache.NMS.MSMQ
{
	public delegate void AcknowledgeHandler(BaseMessage baseMessage);

	public class BaseMessage : IMessage
	{
		#region Acknowledgement

		private event AcknowledgeHandler Acknowledger;
		public void Acknowledge()
		{
			if(null != Acknowledger)
			{
				Acknowledger(this);
			}
		}

		#endregion

		#region Message body

		private byte[] content;
		public byte[] Content
		{
			get { return content; }
			set { this.content = value; }
		}

		private bool readOnlyMsgBody = false;
		/// <summary>
		/// Whether the message body is read-only.
		/// </summary>
		public bool ReadOnlyBody
		{
			get { return readOnlyMsgBody; }
			set { readOnlyMsgBody = value; }
		}

		/// <summary>
		/// Clears out the message body. Clearing a message's body does not clear its header
		/// values or property entries.
		///
		/// If this message body was read-only, calling this method leaves the message body in
		/// the same state as an empty body in a newly created message.
		/// </summary>
		public virtual void ClearBody()
		{
			this.Content = null;
			this.readOnlyMsgBody = false;
		}

		#endregion

		#region Message properties

		private PrimitiveMap propertiesMap = new PrimitiveMap();
		private MessagePropertyIntercepter propertyHelper;
		/// <summary>
		/// Provides access to the message properties (headers)
		/// </summary>
		public Apache.NMS.IPrimitiveMap Properties
		{
			get
			{
                if(propertyHelper == null)
                {
				    propertyHelper = new Apache.NMS.Util.MessagePropertyIntercepter(
					    this, propertiesMap, this.ReadOnlyProperties);
				}

                return propertyHelper;
			}
		}

		private bool readOnlyMsgProperties = false;
		/// <summary>
		/// Whether the message properties is read-only.
		/// </summary>
		public virtual bool ReadOnlyProperties
		{
			get { return this.readOnlyMsgProperties; }

			set
			{
				if(this.propertyHelper != null)
				{
					this.propertyHelper.ReadOnly = value;
				}
				this.readOnlyMsgProperties = value;
			}
		}

		/// <summary>
		/// Clears a message's properties.
		/// The message's header fields and body are not cleared.
		/// </summary>
		public void ClearProperties()
		{
            this.ReadOnlyProperties = false;
            this.propertiesMap.Clear();
		}

		public object GetObjectProperty(string name)
		{
			return Properties[name];
		}

		public void SetObjectProperty(string name, object value)
		{
            Properties[name] = value;
		}

		#endregion

		#region Message header fields

		private string messageId;
		/// <summary>
		/// The message ID which is set by the provider
		/// </summary>
		public string NMSMessageId
		{
			get { return messageId; }
			set { messageId = value; }
		}

		private string correlationId;
		/// <summary>
		/// The correlation ID used to correlate messages with conversations or long running business processes
		/// </summary>
		public string NMSCorrelationID
		{
			get { return correlationId; }
			set { correlationId = value; }
		}

		private IDestination destination;
		/// <summary>
		/// The destination of the message
		/// </summary>
		public IDestination NMSDestination
		{
			get { return destination; }
			set { destination = value; }
		}

		private TimeSpan timeToLive;
		/// <summary>
		/// The time in milliseconds that this message should expire in
		/// </summary>
		public TimeSpan NMSTimeToLive
		{
			get { return timeToLive; }
			set { timeToLive = value; }
		}

		private MsgDeliveryMode deliveryMode;
		/// <summary>
		/// Whether or not this message is persistent
		/// </summary>
		public MsgDeliveryMode NMSDeliveryMode
		{
			get { return deliveryMode; }
			set { deliveryMode = value; }
		}

		private MsgPriority priority;
		/// <summary>
		/// The Priority on this message
		/// </summary>
		public MsgPriority NMSPriority
		{
			get { return priority; }
			set { priority = value; }
		}

		/// <summary>
		/// Returns true if this message has been redelivered to this or another consumer before being acknowledged successfully.
		/// </summary>
		public bool NMSRedelivered
		{
			get { return false; }
            set { }
		}

		private Destination replyTo;
		/// <summary>
		/// The destination that the consumer of this message should send replies to
		/// </summary>
		public IDestination NMSReplyTo
		{
			get { return replyTo; }
			set { replyTo = (Destination) value; }
		}

		private DateTime timestamp = new DateTime();
		/// <summary>
		/// The timestamp the broker added to the message
		/// </summary>
		public DateTime NMSTimestamp
		{
			get { return timestamp; }
			set { timestamp = value; }
		}

		private string type;
		/// <summary>
		/// The type name of this message
		/// </summary>
		public string NMSType
		{
			get { return type; }
			set { type = value; }
		}

        #endregion

        #region Check access mode

		protected void FailIfReadOnlyBody()
		{
			if(ReadOnlyBody == true)
			{
				throw new MessageNotWriteableException("Message is in Read-Only mode.");
			}
		}

		protected void FailIfWriteOnlyBody()
		{
			if(ReadOnlyBody == false)
			{
				throw new MessageNotReadableException("Message is in Write-Only mode.");
			}
		}

        #endregion
	}
}

