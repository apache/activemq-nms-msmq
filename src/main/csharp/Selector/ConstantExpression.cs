using System;
using System.Text;
using System.Globalization;
/**
 *
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Apache.NMS.Selector
{
    /// <summary>
    /// Represents a constant expression.
    /// </summary>
    public class ConstantExpression : IExpression
    {
        private object value;
        public object Value
        {
            get { return value; }
        }    

        public ConstantExpression(object value)
        {
            this.value = value;
        }

        public static ConstantExpression CreateFromDecimal(string text)
        {
    	    // Long integer specified ?
    	    object value;
            if(text.EndsWith("l") || text.EndsWith("L"))
            {
    		    text = text.Substring(0, text.Length - 1);
                value = Int64.Parse(text, CultureInfo.InvariantCulture);
            }
            else
            {
                long lvalue = Int64.Parse(text, CultureInfo.InvariantCulture);
                if(lvalue >= Int32.MinValue && lvalue <= Int32.MaxValue)
                {
                    value = (int)lvalue;
                }
                else
                {
                    value = lvalue;
                }
            }
            return new ConstantExpression(value);
        }

        public static ConstantExpression CreateFromHex(string text)
        {
            long lvalue = Convert.ToInt64(text.Substring(2), 16);

    	    object value;
            if(lvalue >= Int32.MinValue && lvalue <= Int32.MaxValue)
            {
                value = (int)lvalue;
            }
            else
            {
                value = lvalue;
            }
            return new ConstantExpression(value);
        }


        public static ConstantExpression CreateFromOctal(string text)
        {
            long lvalue = Convert.ToInt64(text, 8);

    	    object value;
            if(lvalue >= Int32.MinValue && lvalue <= Int32.MaxValue)
            {
                value = (int)lvalue;
            }
            else
            {
                value = lvalue;
            }
            return new ConstantExpression(value);
        }

        public static ConstantExpression CreateFloat(string text)
        {
            double value = Double.Parse(text, CultureInfo.InvariantCulture);
            return new ConstantExpression(value);
        }

        public object Evaluate(MessageEvaluationContext message)
        {
            return value;
        }

        public override string ToString()
        {
            if(value == null)
            {
                return "NULL";
            }
            if(value is bool)
            {
                return (bool)value ? "TRUE" : "FALSE";
            }
            if(value is string)
            {
                return EncodeString((string)value);
            }
            return value.ToString();
        }

        public override int GetHashCode()
        {
            return (value == null ? 0 : value.GetHashCode());
        }

        /// <summary>
        /// Encodes the value of string so that it looks like it would look like
        /// when it was provided in a selector.
        /// </summary>
        /// <param name="s">String to be encoded.</param>
        /// <return>Encoded string.</return>
        public static string EncodeString(string s)
        {
            StringBuilder b = new StringBuilder();
            b.Append('\'');
            for(int c = 0; c < s.Length; c++)
            {
                char ch = s[c];
                if(ch == '\'')
                {
                    b.Append(ch);
                }
                b.Append(ch);
            }
            b.Append('\'');
            return b.ToString();
        }

        public static readonly BooleanConstantExpression NULL  = new BooleanConstantExpression(null);
        public static readonly BooleanConstantExpression TRUE  = new BooleanConstantExpression(true);
        public static readonly BooleanConstantExpression FALSE = new BooleanConstantExpression(false);
    }
}