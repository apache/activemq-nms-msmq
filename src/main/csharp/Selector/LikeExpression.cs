using System;
using System.Text;
using System.Text.RegularExpressions;
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
    /// A filter performing a string matching comparison.
    /// </summary>
    public class LikeExpression : BooleanUnaryExpression
    {
        private bool notNot;
        private Regex pattern;

        protected override string ExpressionSymbol
        {
            get { return notNot ? "LIKE" : "NOT LIKE"; }
        }

        public LikeExpression(IExpression left, string like, string escape, bool notNot)
            : base(left)
        {
            this.notNot = notNot;

            bool doEscape = false;
            char escapeChar = '%';

            if(escape != null)
            {
                if(escape.Length != 1)
                {
                    throw new ApplicationException("The ESCAPE string litteral is invalid.  It can only be one character.  Litteral used: " + escape);
                }
                doEscape = true;
                escapeChar = escape[0];
            }

            StringBuilder temp = new StringBuilder();
            StringBuilder regexp = new StringBuilder(like.Length * 2);
            regexp.Append("^"); // The beginning of the input
            for(int c = 0; c < like.Length; c++)
            {
                char ch = like[c];
                if(doEscape && (ch == escapeChar))
                {
                    c++;
                    if(c >= like.Length)
                    {
                        // nothing left to escape...
                        break;
                    }
                    temp.Append(like[c]);
                }
                else if(ch == '%')
                {
                    if(temp.Length > 0)
                    {
                        regexp.Append(Regex.Escape(temp.ToString()));
                        temp.Length = 0;
                    }
                    regexp.Append(".*?"); // Do a non-greedy match 
                }
                else if(c == '_')
                {
                    if(temp.Length > 0)
                    {
                        regexp.Append(Regex.Escape(temp.ToString()));
                        temp.Length = 0;
                    }
                    regexp.Append("."); // match one 
                }
                else
                {
                    temp.Append(ch);
                }
            }
            if(temp.Length > 0)
            {
                regexp.Append(Regex.Escape(temp.ToString()));
            }
            regexp.Append("$"); // The end of the input

            pattern = new Regex(regexp.ToString(), RegexOptions.Singleline | RegexOptions.Compiled);
        }

        public override object Evaluate(MessageEvaluationContext message)
        {
            object rvalue = this.Right.Evaluate(message);

            bool answer = false;
            if(rvalue != null)
            {
                if(rvalue is string)
                {
                    answer = pattern.IsMatch((string)rvalue);
                }
                else
                {
                    //throw new ApplicationException("LIKE can only operate on string identifiers. LIKE attemped on " + rvalue.GetType().ToString());
                }
            }

            return notNot ? answer : !answer;
        }
    }
}
