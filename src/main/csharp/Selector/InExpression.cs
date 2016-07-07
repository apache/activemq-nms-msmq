using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
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
    /// A boolean expression which checks if an expression value is
    /// contained in a list of defined values.
    /// </summary>
    public class InExpression : BooleanUnaryExpression
    {
        private bool notNot;
        private ArrayList elements;
        private HashSet<string> hashset;

        protected override string ExpressionSymbol
        {
            get { return notNot ? "IN" : "NOT IN"; }
        }

        public InExpression(IExpression right, ArrayList elements, bool notNot)
            : base(right)
        {
            this.notNot = notNot;

            this.elements = elements;
            this.hashset = new HashSet<string>();

            foreach(object element in elements)
            {
                hashset.Add((string)element);
            }
        }

        public override object Evaluate(MessageEvaluationContext message)
        {
            object rvalue = Right.Evaluate(message);

            bool answer = false;
            if(rvalue != null && (rvalue is string))
            {
                answer = hashset.Contains((string)rvalue);
            }

            return notNot ? answer : !answer;
        }

        public override string ToString()
        {
            StringBuilder answer = new StringBuilder();
            answer.Append(Right);
            answer.Append(" ");
            answer.Append(ExpressionSymbol);
            answer.Append(" (");

            for(int i = 0; i < elements.Count; i++)
            {
                if(i > 0) answer.Append(", ");

                string s = (string)elements[i];

                answer.Append('\'');
                for(int c = 0; c < s.Length; c++)
                {
                    char ch = s[c];
                    if(ch == '\'')
                    {
                        answer.Append(ch);
                    }
                    answer.Append(ch);
                }
                answer.Append('\'');
            }

            answer.Append(")");
            return answer.ToString();
        }
    }
}
