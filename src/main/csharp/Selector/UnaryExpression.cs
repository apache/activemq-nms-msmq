using System;
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
    /// An expression which performs an operation on one expression value.
    /// </summary>
    public abstract class UnaryExpression : IExpression
    {
        protected IExpression rightExpression;
        public IExpression Right
        {
            get { return rightExpression; }
            set { rightExpression = value; }
        }

        protected abstract string ExpressionSymbol
        {
            get;
        }

        public UnaryExpression(IExpression left)
        {
            this.rightExpression = left;
        }

        public abstract object Evaluate(MessageEvaluationContext message);

        public override string ToString()
        {
            return "(" + ExpressionSymbol + " " + rightExpression.ToString() + ")";
        }

        public static IExpression CreateNegate(IExpression left)
        {
            return new NegateExpression(left);
        }

        public static IBooleanExpression CreateNOT(IBooleanExpression left)
        {
            return new NOTExpression(left);
        }

        public static IBooleanExpression CreateBooleanCast(IExpression left)
        {
            return new BooleanCastExpression(left);
        }
    }
}
